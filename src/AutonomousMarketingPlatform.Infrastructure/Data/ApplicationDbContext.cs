using AutonomousMarketingPlatform.Domain.Entities;
using AutonomousMarketingPlatform.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace AutonomousMarketingPlatform.Infrastructure.Data;

/// <summary>
/// Contexto de base de datos principal de la aplicación.
/// Implementa filtrado automático por tenant para garantizar aislamiento de datos.
/// Extiende IdentityDbContext para soportar ASP.NET Core Identity.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly IServiceProvider? _serviceProvider;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor? httpContextAccessor = null, IServiceProvider? serviceProvider = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
    }
    
    // DbSets
    public DbSet<Tenant> Tenants { get; set; }
    // Nota: Users ahora se maneja a través de ApplicationUser (Identity)
    public DbSet<Consent> Consents { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<Content> Contents { get; set; }
    public DbSet<UserPreference> UserPreferences { get; set; }
    public DbSet<MarketingMemory> MarketingMemories { get; set; }
    public DbSet<AutomationState> AutomationStates { get; set; }
    public DbSet<AutomationExecution> AutomationExecutions { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<UserTenant> UserTenants { get; set; }
    public DbSet<MarketingPack> MarketingPacks { get; set; }
    public DbSet<GeneratedCopy> GeneratedCopies { get; set; }
    public DbSet<MarketingAssetPrompt> MarketingAssetPrompts { get; set; }
    public DbSet<CampaignDraft> CampaignDrafts { get; set; }
    public DbSet<TenantAIConfig> TenantAIConfigs { get; set; }
    public DbSet<TenantN8nConfig> TenantN8nConfigs { get; set; }
    public DbSet<PublishingJob> PublishingJobs { get; set; }
    public DbSet<CampaignMetrics> CampaignMetrics { get; set; }
    public DbSet<PublishingJobMetrics> PublishingJobMetrics { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Si no está configurado en Program.cs, usar cadena de conexión directamente
        if (!optionsBuilder.IsConfigured)
        {
            // Intentar obtener desde variable de entorno primero
            var envConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            
            // Si no está en variable de entorno, intentar desde appsettings
            if (string.IsNullOrEmpty(envConnectionString))
            {
                var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "AutonomousMarketingPlatform.Web");
                if (!Directory.Exists(basePath))
                {
                    basePath = Directory.GetCurrentDirectory();
                }
                
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: true)
                    .Build();
                
                envConnectionString = configuration.GetConnectionString("DefaultConnection");
            }
            
            if (!string.IsNullOrEmpty(envConnectionString))
            {
                optionsBuilder.UseNpgsql(envConnectionString);
            }
        }
        
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configurar Identity para usar Guid como clave y nombres de tabla personalizados
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("AspNetUsers");
            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            
            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("AspNetRoles");
        });
        
        modelBuilder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.ToTable("AspNetUserRoles");
        });
        
        modelBuilder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("AspNetUserClaims");
        });
        
        modelBuilder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("AspNetUserLogins");
        });
        
        modelBuilder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("AspNetUserTokens");
        });
        
        modelBuilder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("AspNetRoleClaims");
        });

        // Configuración de Tenant
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Subdomain).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Subdomain).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ContactEmail).IsRequired().HasMaxLength(255);
        });

        // Nota: La entidad User (dominio) fue eliminada.
        // Los usuarios ahora se manejan exclusivamente a través de ApplicationUser (Identity)

        // Configuración de Consent
        modelBuilder.Entity<Consent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.UserId, e.ConsentType });
            entity.Property(e => e.ConsentType).IsRequired().HasMaxLength(100);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de Campaign
        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Status });
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Budget).HasPrecision(18, 2);
            entity.Property(e => e.Objectives).HasMaxLength(2000);
            entity.Property(e => e.TargetAudience).HasMaxLength(2000);
            entity.Property(e => e.TargetChannels).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(5000);
            
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Campaigns)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de PublishingJob
        modelBuilder.Entity<PublishingJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.CampaignId, e.Status });
            entity.HasIndex(e => new { e.TenantId, e.ScheduledDate });
            entity.Property(e => e.Channel).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Content).HasMaxLength(5000);
            entity.Property(e => e.Hashtags).HasMaxLength(500);
            entity.Property(e => e.PublishedUrl).HasMaxLength(1000);
            entity.Property(e => e.ExternalPostId).HasMaxLength(200);
            entity.Property(e => e.MediaUrl).HasMaxLength(1000);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.Payload).HasMaxLength(10000);
            entity.Property(e => e.DownloadUrl).HasMaxLength(2000);
            
            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.PublishingJobs)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.MarketingPack)
                .WithMany()
                .HasForeignKey(e => e.MarketingPackId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.GeneratedCopy)
                .WithMany()
                .HasForeignKey(e => e.GeneratedCopyId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.MarketingAssetPrompt)
                .WithMany()
                .HasForeignKey(e => e.MarketingAssetPromptId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuración de CampaignMetrics
        modelBuilder.Entity<CampaignMetrics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.CampaignId, e.MetricDate }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.MetricDate });
            entity.Property(e => e.Source).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            
            entity.HasOne(e => e.Campaign)
                .WithMany()
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de PublishingJobMetrics
        modelBuilder.Entity<PublishingJobMetrics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.PublishingJobId, e.MetricDate }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.MetricDate });
            entity.Property(e => e.Source).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            
            entity.HasOne(e => e.PublishingJob)
                .WithMany()
                .HasForeignKey(e => e.PublishingJobId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de Content
        modelBuilder.Entity<Content>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FileUrl).IsRequired().HasMaxLength(1000);
            
            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.Contents)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuración de UserPreference
        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.UserId, e.PreferenceKey });
            entity.Property(e => e.PreferenceKey).IsRequired().HasMaxLength(100);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de MarketingMemory
        modelBuilder.Entity<MarketingMemory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MemoryType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Content).IsRequired();
            
            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.MarketingMemories)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuración de AutomationState
        modelBuilder.Entity<AutomationState>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AutomationType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            
            entity.HasOne(e => e.Campaign)
                .WithMany()
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuración de AutomationExecution
        modelBuilder.Entity<AutomationExecution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RequestId).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.Status });
            entity.Property(e => e.RequestId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.WorkflowId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
        });

        // Configuración de AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.CreatedAt });
            entity.HasIndex(e => new { e.TenantId, e.Action, e.EntityType });
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Result).IsRequired().HasMaxLength(50);
        });

        // Configuración de UserTenant
        modelBuilder.Entity<UserTenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.TenantId }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.RoleId });
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserTenants)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de MarketingPack
        modelBuilder.Entity<MarketingPack>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.ContentId });
            entity.HasIndex(e => new { e.TenantId, e.Status });
            entity.Property(e => e.Strategy).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            
            entity.HasOne(e => e.Content)
                .WithMany()
                .HasForeignKey(e => e.ContentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.MarketingPacks)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });

        // Configuración de GeneratedCopy
        modelBuilder.Entity<GeneratedCopy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.MarketingPackId });
            entity.Property(e => e.CopyType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Content).IsRequired();
            
            entity.HasOne(e => e.MarketingPack)
                .WithMany(mp => mp.Copies)
                .HasForeignKey(e => e.MarketingPackId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de MarketingAssetPrompt
        modelBuilder.Entity<MarketingAssetPrompt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.MarketingPackId });
            entity.Property(e => e.AssetType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Prompt).IsRequired();
            
            entity.HasOne(e => e.MarketingPack)
                .WithMany(mp => mp.AssetPrompts)
                .HasForeignKey(e => e.MarketingPackId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de CampaignDraft
        modelBuilder.Entity<CampaignDraft>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Status });
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            
            entity.HasOne(e => e.MarketingPack)
                .WithMany()
                .HasForeignKey(e => e.MarketingPackId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.ConvertedCampaign)
                .WithMany()
                .HasForeignKey(e => e.ConvertedCampaignId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Aplicar índices para mejorar rendimiento en consultas multi-tenant
        ApplyTenantIndexes(modelBuilder);
    }

    /// <summary>
    /// Aplica índices en TenantId para todas las entidades que implementan ITenantEntity.
    /// Esto mejora significativamente el rendimiento de las consultas filtradas por tenant.
    /// </summary>
    private void ApplyTenantIndexes(ModelBuilder modelBuilder)
    {
        // Nota: User (dominio) fue eliminado, ahora se usa ApplicationUser (Identity)
        modelBuilder.Entity<Consent>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<Campaign>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<Content>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<UserPreference>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<MarketingMemory>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<AutomationState>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<AutomationExecution>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<AuditLog>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<MarketingPack>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<GeneratedCopy>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<MarketingAssetPrompt>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<CampaignDraft>().HasIndex(e => e.TenantId);
        // Configuración de TenantAIConfig
        modelBuilder.Entity<TenantAIConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Provider }).IsUnique();
            entity.Property(e => e.Provider).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Model).IsRequired().HasMaxLength(100);
            
            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de TenantN8nConfig
        modelBuilder.Entity<TenantN8nConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TenantId).IsUnique();
            entity.Property(e => e.BaseUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ApiUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.DefaultWebhookUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.WebhookUrlsJson).HasColumnType("text");
            
            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PublishingJob>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<CampaignMetrics>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<PublishingJobMetrics>().HasIndex(e => e.TenantId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Obtener TenantService de forma lazy para evitar dependencia circular
        Guid? tenantId = null;
        
        if (_serviceProvider != null)
        {
            try
            {
                var tenantService = _serviceProvider.GetService(typeof(ITenantService)) as ITenantService;
                tenantId = tenantService?.GetCurrentTenantId();
            }
            catch
            {
                // Si no está disponible (tiempo de diseño), continuar sin asignar TenantId
            }
        }

        // Solo asignar TenantId si está disponible
        if (tenantId.HasValue)
        {

            // Asegurar que todas las entidades ITenantEntity tengan TenantId asignado
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Domain.Common.ITenantEntity && e.State == EntityState.Added);

            foreach (var entry in entries)
            {
                if (entry.Entity is Domain.Common.ITenantEntity tenantEntity)
                {
                    if (tenantEntity.TenantId == Guid.Empty && tenantId.HasValue)
                    {
                        tenantEntity.TenantId = tenantId.Value;
                    }
                }
            }
        }

        // Actualizar UpdatedAt para entidades modificadas
        var modifiedEntries = ChangeTracker.Entries()
            .Where(e => e.Entity is Domain.Common.BaseEntity && 
                       (e.State == EntityState.Modified || e.State == EntityState.Added));

        foreach (var entry in modifiedEntries)
        {
            if (entry.Entity is Domain.Common.BaseEntity baseEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    baseEntity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    baseEntity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

