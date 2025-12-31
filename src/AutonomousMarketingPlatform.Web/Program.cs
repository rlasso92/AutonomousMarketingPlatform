using AutonomousMarketingPlatform.Application.Services;
using AutonomousMarketingPlatform.Application.UseCases.Campaigns;
using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using AutonomousMarketingPlatform.Infrastructure.Data;
using AutonomousMarketingPlatform.Infrastructure.Repositories;
using AutonomousMarketingPlatform.Infrastructure.Services;
using AutonomousMarketingPlatform.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Linq;

// ============================================
// INICIO DE LA APLICACIÓN - LOGGING TEMPRANO
// ============================================
Console.WriteLine("============================================");
Console.WriteLine("INICIANDO Autonomous Marketing Platform");
Console.WriteLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
Console.WriteLine($"Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Not Set"}");
Console.WriteLine("============================================");

var builder = WebApplication.CreateBuilder(args);

// Configurar puerto para Render (lee PORT de variable de entorno)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    Console.WriteLine($"[INFO] Configurando puerto desde variable de entorno: {port}");
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}
else
{
    Console.WriteLine("[WARNING] Variable de entorno PORT no encontrada, usando configuración por defecto");
}

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configurar Entity Framework Core con PostgreSQL
// Prioridad: Variable de entorno > appsettings.Production.json > appsettings.json
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    var errorMsg = "Connection string 'DefaultConnection' not found. " +
        "Please configure 'ConnectionStrings__DefaultConnection' environment variable or add it to appsettings.json";
    Console.WriteLine($"[ERROR] {errorMsg}");
    Console.WriteLine("[ERROR] La aplicación no puede iniciar sin una cadena de conexión válida.");
    throw new InvalidOperationException(errorMsg);
}

var connectionSource = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") != null 
    ? "Environment Variable" 
    : "Configuration File";
Console.WriteLine($"[INFO] Connection string obtenida desde: {connectionSource}");
Console.WriteLine($"[INFO] Connection string (oculta): Host=***, Database=***");

// Registrar servicios de infraestructura primero (para evitar dependencia circular)
builder.Services.AddHttpContextAccessor();

// Agregar DbContextFactory para TenantService (sin dependencia circular)
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configurar DbContext con acceso a IServiceProvider
// Usamos AddDbContextFactory para evitar dependencia circular en el registro
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Registrar ApplicationDbContext manualmente para pasar IServiceProvider
builder.Services.AddScoped<ApplicationDbContext>(serviceProvider =>
{
    var factory = serviceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    var dbContext = factory.CreateDbContext();
    
    // Inyectar IServiceProvider e IHttpContextAccessor usando reflection o simplemente usar el factory
    // Para evitar la dependencia circular, usamos el factory directamente
    var httpContextAccessor = serviceProvider.GetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
    
    // Crear nuevo contexto con los servicios necesarios
    var options = serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
    return new ApplicationDbContext(options, httpContextAccessor, serviceProvider);
});

// Registrar TenantService (usa IDbContextFactory, no ApplicationDbContext directamente)
builder.Services.AddScoped<ITenantService, TenantService>();

// Registrar repositorios
builder.Services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

// Registrar UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Registrar MediatR para CQRS
// Usar el assembly directamente en lugar de cargarlo por nombre
var applicationAssembly = typeof(AutonomousMarketingPlatform.Application.UseCases.Campaigns.CreateCampaignCommand).Assembly;
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

// Registrar FluentValidation manualmente
builder.Services.AddScoped<FluentValidation.IValidator<AutonomousMarketingPlatform.Application.DTOs.CreateCampaignDto>, 
    AutonomousMarketingPlatform.Application.Validators.CreateCampaignDtoValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<AutonomousMarketingPlatform.Application.DTOs.UpdateCampaignDto>, 
    AutonomousMarketingPlatform.Application.Validators.UpdateCampaignDtoValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<AutonomousMarketingPlatform.Application.DTOs.RegisterCampaignMetricsDto>, 
    AutonomousMarketingPlatform.Application.Validators.RegisterCampaignMetricsDtoValidator>();
builder.Services.AddScoped<FluentValidation.IValidator<AutonomousMarketingPlatform.Application.DTOs.RegisterPublishingJobMetricsDto>, 
    AutonomousMarketingPlatform.Application.Validators.RegisterPublishingJobMetricsDtoValidator>();

// Registrar servicios de consentimiento
builder.Services.AddScoped<IConsentValidationService, ConsentValidationService>();

// Registrar servicios de almacenamiento de archivos
builder.Services.AddScoped<IFileStorageService>(serviceProvider =>
{
    var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
    var logger = serviceProvider.GetRequiredService<ILogger<FileStorageService>>();
    return new FileStorageService(environment.WebRootPath ?? throw new InvalidOperationException("WebRootPath no configurado"), logger);
});

// Registrar servicios de memoria de marketing
builder.Services.AddScoped<IMarketingMemoryService, MarketingMemoryService>();

// Registrar servicios de seguridad y auditoría
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Registrar servicio de encriptación
builder.Services.AddScoped<IEncryptionService, EncryptionService>();

// Registrar servicio de automatizaciones externas
// Configurar HttpClient para ExternalAutomationService (n8n)
builder.Services.AddHttpClient<ExternalAutomationService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    var n8nBaseUrl = builder.Configuration["N8n:BaseUrl"] ?? "http://localhost:5678";
    client.BaseAddress = new Uri(n8nBaseUrl);
});

builder.Services.AddScoped<IExternalAutomationService, ExternalAutomationService>();

// Registrar proveedor de IA
builder.Services.AddHttpClient<IAIProvider, AutonomousMarketingPlatform.Infrastructure.Services.AI.OpenAIProvider>();

// Registrar servicios de publicación
builder.Services.AddSingleton<AutonomousMarketingPlatform.Domain.Interfaces.IPublishingAdapter, 
    AutonomousMarketingPlatform.Infrastructure.Services.Publishing.ManualPublishingAdapter>();

// Registrar background service para procesar cola de publicaciones (también implementa IPublishingJobService)
builder.Services.AddSingleton<AutonomousMarketingPlatform.Infrastructure.Services.Publishing.PublishingJobProcessorService>();
builder.Services.AddSingleton<AutonomousMarketingPlatform.Application.Services.IPublishingJobService>(
    serviceProvider => serviceProvider.GetRequiredService<AutonomousMarketingPlatform.Infrastructure.Services.Publishing.PublishingJobProcessorService>());
builder.Services.AddHostedService(serviceProvider => 
    serviceProvider.GetRequiredService<AutonomousMarketingPlatform.Infrastructure.Services.Publishing.PublishingJobProcessorService>());

// Registrar servicios de métricas
builder.Services.AddScoped<AutonomousMarketingPlatform.Application.Services.IMetricsService, 
    AutonomousMarketingPlatform.Infrastructure.Services.MetricsService>();

// Registrar servicios de aprendizaje automático
builder.Services.AddScoped<AutonomousMarketingPlatform.Application.Services.IMemoryLearningService, 
    AutonomousMarketingPlatform.Infrastructure.Services.MemoryLearningService>();

// Registrar background service para aprendizaje automático desde métricas
builder.Services.AddHostedService<AutonomousMarketingPlatform.Infrastructure.Services.MetricsLearningBackgroundService>();

// Configurar ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Configuración de contraseña
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
    // Configuración de usuario
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    
    // Configuración de lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // Configuración de sign-in
    options.SignIn.RequireConfirmedEmail = false; // Para MVP, luego activar
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configurar cookies de autenticación
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "AutonomousMarketingPlatform.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsProduction() 
        ? CookieSecurePolicy.Always 
        : CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// NOTA: DataProtection Keys
// ASP.NET Core Identity usa DataProtection automáticamente para proteger cookies y tokens.
// En Render, los contenedores son efímeros, por lo que las keys se regeneran en cada reinicio.
// Esto es aceptable para MVP. Para producción multi-instancia, se debe implementar:
// - Persistencia en Redis o base de datos
// - O usar un servicio de DataProtection compartido
// Por ahora, NO se configura persistencia para evitar dependencias adicionales.

// Registrar TenantResolverService
builder.Services.AddScoped<ITenantResolverService, TenantResolverService>();

// Registrar RoleSeeder y UserSeeder
builder.Services.AddScoped<RoleSeeder>();
builder.Services.AddScoped<UserSeeder>();

// Registrar servicio de logging persistente PRIMERO (Singleton para que el proveedor pueda usarlo)
// Debe registrarse antes del proveedor para que esté disponible cuando se cree
builder.Services.AddSingleton<ILoggingService, LoggingService>();

// Configurar logging estructurado
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// El proveedor de logging a base de datos se agregará después de construir la app
// para tener acceso a los servicios necesarios (ILoggingService, IHttpContextAccessor)

if (builder.Environment.IsProduction())
{
    // En producción, agregar Application Insights o similar
    // builder.Logging.AddApplicationInsights();
}

// Configurar ForwardedHeaders para reverse proxy (Render)
// IMPORTANTE: Debe configurarse ANTES de cualquier middleware que use Request.Scheme o Request.Host
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | 
                                ForwardedHeaders.XForwardedProto | 
                                ForwardedHeaders.XForwardedHost;
    
    // En Render, confiar en todos los proxies (Render maneja la seguridad)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    
    // Limitar a un número razonable de proxies
    options.ForwardLimit = 1;
});

// Configurar CORS de forma segura
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // En desarrollo, permitir cualquier origen
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    }
    else
    {
        // En producción, solo orígenes específicos
        options.AddDefaultPolicy(policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                ?? Array.Empty<string>();
            
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    }
});

Console.WriteLine("[INFO] Construyendo aplicación...");
WebApplication app;
try
{
    app = builder.Build();
    Console.WriteLine("[INFO] Aplicación construida exitosamente");
    
    // Agregar el proveedor de logging a la base de datos al sistema de logging
    // Esto debe hacerse después de construir la app para tener acceso al servicio
    var loggingService = app.Services.GetRequiredService<ILoggingService>();
    var httpContextAccessor = app.Services.GetService<IHttpContextAccessor>();
    var databaseLoggerProvider = new AutonomousMarketingPlatform.Infrastructure.Logging.DatabaseLoggerProvider(
        loggingService, 
        httpContextAccessor);
    app.Services.GetRequiredService<ILoggerFactory>().AddProvider(databaseLoggerProvider);
    Console.WriteLine("[INFO] Proveedor de logging a base de datos agregado exitosamente");
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] Error al construir la aplicación: {ex.GetType().Name}");
    Console.WriteLine($"[ERROR] Mensaje: {ex.Message}");
    Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"[ERROR] InnerException: {ex.InnerException.GetType().Name}");
        Console.WriteLine($"[ERROR] InnerException Message: {ex.InnerException.Message}");
    }
    throw;
}

// Seed roles (siempre, en desarrollo y producción)
Console.WriteLine("[INFO] Iniciando seeding de datos del sistema...");
using (var scope = app.Services.CreateScope())
{
    try
    {
        Console.WriteLine("[INFO] Seeding roles...");
        var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
        await roleSeeder.SeedRolesAsync();
        Console.WriteLine("[INFO] Roles seeded exitosamente");

        // Crear tenant por defecto si no existe (siempre, para que funcione sin subdominio)
        Console.WriteLine("[INFO] Verificando/creando tenant por defecto...");
        var userSeeder = scope.ServiceProvider.GetRequiredService<UserSeeder>();
        var defaultTenant = await userSeeder.CreateTestTenantAsync(
            name: "Tenant por Defecto",
            subdomain: "default",
            contactEmail: "default@autonomousmarketingplatform.com");
        
        if (defaultTenant != null)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Tenant por defecto verificado/creado: {TenantId}", defaultTenant.Id);
            Console.WriteLine($"[INFO] Tenant por defecto: {defaultTenant.Id}");
        }
        else
        {
            Console.WriteLine("[WARNING] No se pudo crear/verificar tenant por defecto");
        }
        Console.WriteLine("[INFO] Seeding de datos del sistema completado");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al inicializar datos del sistema");
        Console.WriteLine($"[ERROR] Error en seeding: {ex.GetType().Name}");
        Console.WriteLine($"[ERROR] Mensaje: {ex.Message}");
        Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"[ERROR] InnerException: {ex.InnerException.GetType().Name}");
            Console.WriteLine($"[ERROR] InnerException Message: {ex.InnerException.Message}");
        }
        // NO lanzar excepción aquí, permitir que la app inicie aunque falle el seeding
    }
}

// Seed roles y usuario inicial (solo en desarrollo)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            // Crear tenant y usuario de prueba
            var userSeeder = scope.ServiceProvider.GetRequiredService<UserSeeder>();
            
            // Crear tenant de prueba
            var tenant = await userSeeder.CreateTestTenantAsync(
                name: "Tenant de Prueba",
                subdomain: "test",
                contactEmail: "test@example.com");

            if (tenant != null)
            {
                // Verificar si admin@test.com ya existe como super admin
                // Si existe con TenantId = Guid.Empty, no crear otro
                var existingAdmin = await scope.ServiceProvider
                    .GetRequiredService<UserManager<ApplicationUser>>()
                    .FindByEmailAsync("admin@test.com");
                
                if (existingAdmin == null || existingAdmin.TenantId != Guid.Empty)
                {
                    // Crear usuario Owner solo si no existe o no es super admin
                    await userSeeder.CreateInitialUserAsync(
                        email: "admin@test.com",
                        password: "Admin123!",
                        fullName: "Administrador de Prueba",
                        tenantId: tenant.Id,
                        roleName: "Owner");
                }

                // Crear usuario Marketer
                await userSeeder.CreateInitialUserAsync(
                    email: "marketer@test.com",
                    password: "Marketer123!",
                    fullName: "Marketer de Prueba",
                    tenantId: tenant.Id,
                    roleName: "Marketer");
            }
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error al inicializar datos de prueba");
        }
    }
}

// Configure the HTTP request pipeline.
Console.WriteLine("[INFO] Configurando pipeline de middlewares...");

// ============================================
// ORDEN CRÍTICO DE MIDDLEWARES PARA RENDER
// ============================================

// 1. ForwardedHeaders (PRIMERO - antes de cualquier middleware que use Request.Scheme/Host)
// Esto permite que ASP.NET Core entienda que está detrás del reverse proxy de Render
Console.WriteLine("[INFO] Configurando ForwardedHeaders...");
app.UseForwardedHeaders();

// 2. Middleware de logging de requests (para debugging en Render)
app.Use(async (context, next) =>
{
    var startTime = DateTime.UtcNow;
    var path = context.Request.Path;
    var method = context.Request.Method;
    var requestId = context.TraceIdentifier;
    
    Console.WriteLine($"[REQUEST] {method} {path} | RequestId={requestId} | {startTime:yyyy-MM-dd HH:mm:ss} UTC");
    
    try
    {
        await next();
        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
        Console.WriteLine($"[RESPONSE] {method} {path} | Status={context.Response.StatusCode} | Duration={duration:F2}ms | RequestId={requestId}");
    }
    catch (Exception ex)
    {
        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
        Console.WriteLine($"[ERROR] {method} {path} | Exception={ex.GetType().Name} | Message={ex.Message} | Duration={duration:F2}ms | RequestId={requestId}");
        Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"[ERROR] InnerException: {ex.InnerException.GetType().Name} | Message={ex.InnerException.Message}");
        }
        throw; // Re-lanzar para que otro middleware lo maneje
    }
});

// 3. Manejo global de excepciones
// TEMPORALMENTE DESACTIVADO PARA DEBUGGING - usando logging directo en consola
// app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// 4. Headers de seguridad
app.UseMiddleware<SecurityHeadersMiddleware>();

// 5. NO usar UseHttpsRedirection() ni UseHsts() en Render
// Render ya maneja HTTPS en el reverse proxy, forzar redirección causa:
// - "Failed to determine the https port for redirect"
// - Loops de redirección
// - Health checks fallan
// Comentado explícitamente:
// if (!app.Environment.IsDevelopment())
// {
//     app.UseHsts();
//     app.UseHttpsRedirection();
// }

// 6. Static files
app.UseStaticFiles();

// 7. Routing (debe ir antes de Authentication/Authorization)
app.UseRouting();

// 8. CORS (después de Routing, antes de Authentication)
app.UseCors();

// 9. Authentication (debe ir después de UseRouting)
app.UseAuthentication();

// 10. Tenant Resolver (después de auth, para poder leer claims)
app.Use(async (context, next) =>
{
    try
    {
        var tenantResolver = context.RequestServices.GetRequiredService<ITenantResolverService>();
        var tenantId = await tenantResolver.ResolveTenantIdAsync();
        if (tenantId.HasValue)
        {
            context.Items["TenantId"] = tenantId.Value;
        }
        else
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("TenantResolver no pudo resolver tenant: Path={Path}, Host={Host}", 
                context.Request.Path, context.Request.Host.Host);
            // En producción, también loguear a consola para debugging
            Console.WriteLine($"[WARNING] TenantResolver no pudo resolver tenant: Path={context.Request.Path}, Host={context.Request.Host.Host}");
        }
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error en TenantResolver middleware: Path={Path}", context.Request.Path);
        // Log también a consola para debugging en Render
        Console.WriteLine($"[ERROR] TenantResolver middleware exception: {ex.GetType().Name}");
        Console.WriteLine($"[ERROR] Mensaje: {ex.Message}");
        Console.WriteLine($"[ERROR] Path: {context.Request.Path}");
        // No lanzar excepción aquí, dejar que el middleware de validación maneje
    }
    await next();
});

// 11. Validación de tenant (después de auth, para poder verificar super admin)
app.UseMiddleware<TenantValidationMiddleware>();

// 12. Middleware de validación de consentimientos
app.UseConsentValidation();

// 13. Authorization (DEBE ir después de UseRouting y ANTES de MapControllerRoute)
app.UseAuthorization();

// 14. Endpoint raíz público para health checks de Render
// IMPORTANTE: Debe estar ANTES de MapControllers/MapControllerRoute para tener prioridad
// Los endpoints mínimos tienen prioridad sobre el routing convencional
Console.WriteLine("[INFO] Configurando endpoint raíz público...");
app.MapGet("/", () => 
{
    Console.WriteLine($"[INFO] Request recibido en endpoint raíz: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
    return Results.Ok(new { 
        status = "ok", 
        service = "Autonomous Marketing Platform",
        timestamp = DateTime.UtcNow 
    });
}).AllowAnonymous().WithName("RootHealthCheck");

// 15. Map controllers y Razor Pages
// IMPORTANTE: Usamos MapControllers() que solo mapea controllers con atributos de ruta
// Luego agregamos MapControllerRoute para rutas convencionales, pero el endpoint "/" ya está mapeado arriba
Console.WriteLine("[INFO] Mapeando rutas de controllers y Razor Pages...");
app.MapControllers(); // Mapea controllers con atributos [Route]

// Mapear rutas convencionales, pero "/" ya está capturado por el endpoint mínimo arriba
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

Console.WriteLine("[INFO] Pipeline de middlewares configurado exitosamente");
Console.WriteLine("[INFO] Iniciando servidor web...");
Console.WriteLine("============================================");

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("============================================");
    Console.WriteLine($"[ERROR CRÍTICO] Error al iniciar el servidor: {ex.GetType().Name}");
    Console.WriteLine($"[ERROR CRÍTICO] Mensaje: {ex.Message}");
    Console.WriteLine($"[ERROR CRÍTICO] StackTrace: {ex.StackTrace}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"[ERROR CRÍTICO] InnerException: {ex.InnerException.GetType().Name}");
        Console.WriteLine($"[ERROR CRÍTICO] InnerException Message: {ex.InnerException.Message}");
        Console.WriteLine($"[ERROR CRÍTICO] InnerException StackTrace: {ex.InnerException.StackTrace}");
    }
    Console.WriteLine("============================================");
    throw;
}

