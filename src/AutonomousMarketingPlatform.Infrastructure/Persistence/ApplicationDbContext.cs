using AutonomousMarketingPlatform.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutonomousMarketingPlatform.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable(name: "Users");
        });

        builder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable(name: "Roles");
        });

        // Rename other identity tables if desired, e.g.:
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>(entity => entity.ToTable("UserRoles"));
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>(entity => entity.ToTable("UserClaims"));
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>(entity => entity.ToTable("UserLogins"));
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>(entity => entity.ToTable("RoleClaims"));
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>(entity => entity.ToTable("UserTokens"));

        builder.Entity<UserTenant>(entity =>
        {
            entity.ToTable("UserTenants");
            entity.HasKey(ut => new { ut.ApplicationUserId, ut.TenantId });
        });
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<UserTenant> UserTenants { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    public DbSet<Content> Content { get; set; }
    public DbSet<Consent> Consents { get; set; }
    public DbSet<MarketingPack> MarketingPacks { get; set; }
    public DbSet<PublishJob> PublishJobs { get; set; }
    public DbSet<MarketingMemory> MarketingMemories { get; set; }
    public DbSet<CampaignMetrics> CampaignMetrics { get; set; }
    public DbSet<PublishJobMetrics> PublishJobMetrics { get; set; }
}
