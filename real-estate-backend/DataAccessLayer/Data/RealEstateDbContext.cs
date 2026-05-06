using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using DataAccessLayer.Entities;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.AIModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.CommunicationModule;
using DataAccessLayer.Entities.Identity;

namespace DataAccessLayer.Data;

public class RealEstateDbContext(DbContextOptions<RealEstateDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Mirrored Pattern: Using Assembly Discovery for configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<Building> Buildings { get; set; }
    public DbSet<Unit> Units { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<Insurance> Insurance { get; set; }
    public DbSet<ProjectFeature> ProjectFeatures { get; set; }
    public DbSet<UnitFeature> UnitFeatures { get; set; }
    public DbSet<ProjectInsurance> ProjectInsurance { get; set; }
    public DbSet<UnitInsurance> UnitInsurance { get; set; }
    public DbSet<ProjectMedia> ProjectMedia { get; set; }
    public DbSet<UnitMedia> UnitMedia { get; set; }
    public DbSet<NearbyFacility> NearbyFacilities { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<BlogImage> BlogImages { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<KnowledgeBaseEntry> KnowledgeBase { get; set; }
    public DbSet<ChatHistory> ChatHistory { get; set; }
}
