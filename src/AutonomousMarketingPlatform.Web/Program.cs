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

var builder = WebApplication.CreateBuilder(args);

// Configurar puerto para Render (lee PORT de variable de entorno)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
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
    throw new InvalidOperationException(errorMsg);
}

Console.WriteLine($"[INFO] Using connection string from: {(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") != null ? "Environment Variable" : "Configuration File")}");

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

// Registrar TenantResolverService
builder.Services.AddScoped<ITenantResolverService, TenantResolverService>();

// Registrar RoleSeeder y UserSeeder
builder.Services.AddScoped<RoleSeeder>();
builder.Services.AddScoped<UserSeeder>();

// Configurar logging estructurado
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

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

var app = builder.Build();

// Seed roles (siempre, en desarrollo y producción)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
        await roleSeeder.SeedRolesAsync();

        // Crear tenant por defecto si no existe (siempre, para que funcione sin subdominio)
        var userSeeder = scope.ServiceProvider.GetRequiredService<UserSeeder>();
        var defaultTenant = await userSeeder.CreateTestTenantAsync(
            name: "Tenant por Defecto",
            subdomain: "default",
            contactEmail: "default@autonomousmarketingplatform.com");
        
        if (defaultTenant != null)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Tenant por defecto verificado/creado: {TenantId}", defaultTenant.Id);
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al inicializar datos del sistema");
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

// ============================================
// ORDEN CRÍTICO DE MIDDLEWARES PARA RENDER
// ============================================

// 1. ForwardedHeaders (PRIMERO - antes de cualquier middleware que use Request.Scheme/Host)
// Esto permite que ASP.NET Core entienda que está detrás del reverse proxy de Render
app.UseForwardedHeaders();

// 2. Manejo global de excepciones
// TEMPORALMENTE DESACTIVADO PARA DEBUGGING
// app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// 3. Headers de seguridad
app.UseMiddleware<SecurityHeadersMiddleware>();

// 4. NO usar UseHttpsRedirection() ni UseHsts() en Render
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

// 5. Static files
app.UseStaticFiles();

// 6. Routing (debe ir antes de Authentication/Authorization)
app.UseRouting();

// 7. CORS (después de Routing, antes de Authentication)
app.UseCors();

// 8. Authentication (debe ir después de UseRouting)
app.UseAuthentication();

// 9. Tenant Resolver (después de auth, para poder leer claims)
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
        }
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error en TenantResolver middleware: Path={Path}", context.Request.Path);
        // No lanzar excepción aquí, dejar que el middleware de validación maneje
    }
    await next();
});

// 10. Validación de tenant (después de auth, para poder verificar super admin)
app.UseMiddleware<TenantValidationMiddleware>();

// 11. Middleware de validación de consentimientos
app.UseConsentValidation();

// 12. Authorization (DEBE ir después de UseRouting y ANTES de MapControllerRoute)
app.UseAuthorization();

// 13. Endpoint raíz público para health checks de Render
// IMPORTANTE: Debe estar ANTES de MapControllerRoute para tener prioridad
app.MapGet("/", () => Results.Ok(new { 
    status = "ok", 
    service = "Autonomous Marketing Platform",
    timestamp = DateTime.UtcNow 
})).AllowAnonymous();

// 14. Map controllers y Razor Pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

