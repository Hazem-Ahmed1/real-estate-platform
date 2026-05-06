using DataAccessLayer.Contracts;
using DataAccessLayer.Data;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Enums;
using DataAccessLayer.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccessLayer.Implementation.Seed;

public class DataSeeder(
    RealEstateDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration
) : IDataSeeder
{
    private const string AdminRoleName = "Admin";

    public async Task SeedAsync()
    {
        await SeedIdentityAsync();
        await SeedProjectsAsync();
    }

    private async Task SeedIdentityAsync()
    {
        if (!await roleManager.RoleExistsAsync(AdminRoleName))
        {
            await roleManager.CreateAsync(new IdentityRole(AdminRoleName));
        }

        var adminUserName = configuration["AdminSeed:UserName"];
        var adminEmail = configuration["AdminSeed:Email"];
        var adminPassword = configuration["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(adminUserName) ||
            string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        var admin = await userManager.FindByNameAsync(adminUserName);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminUserName,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var created = await userManager.CreateAsync(admin, adminPassword);
            if (!created.Succeeded)
            {
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(admin, AdminRoleName))
        {
            await userManager.AddToRoleAsync(admin, AdminRoleName);
        }
    }

    private async Task SeedProjectsAsync()
    {
        if (await context.Projects.AnyAsync()) return;

        // 1. Seed Features (Lookups)
        var features = new List<Feature>
        {
            new() { Name = "مسبح أوليمبي" },
            new() { Name = "نادي صحي (Gym)" },
            new() { Name = "مساحات خضراء واسعة" },
            new() { Name = "منطقة ألعاب أطفال" },
            new() { Name = "أمن وحراسة 24/7" },
            new() { Name = "جراج تحت الأرض" },
            new() { Name = "تكييف مركزي" }
        };
        await context.Features.AddRangeAsync(features);
        await context.SaveChangesAsync();

        // 2. Seed Projects with Buildings and Units
        var projects = new List<Project>
        {
            new()
            {
                Name = "كمبوند لؤلؤة القاهرة",
                Status = ProjectStatus.ForSale,
                TotalArea = 50000,
                City = "القاهرة",
                Area = "التجمع الخامس",
                Address = "شارع التسعين الشمالي، التجمع الخامس",
                Latitude = 30.0263,
                Longitude = 31.4925,
                Description = "مشروع سكني فاخر يضم كافة الخدمات الرفاهية في قلب القاهرة الجديدة.",
                Buildings = new List<Building>
                {
                    new() 
                    { 
                        Name = "عمارة A1", Floors = 5,
                        Units = new List<Unit>
                        {
                            new() { Type = UnitType.Apartment, Status = UnitStatus.ForSale, Price = 2500000, Area = 150, Rooms = 3, Bathrooms = 2, Floor = 2 },
                            new() { Type = UnitType.Apartment, Status = UnitStatus.Sold, Price = 3000000, Area = 180, Rooms = 4, Bathrooms = 3, Floor = 4 }
                        }
                    },
                    new() 
                    { 
                        Name = "عمارة A2", Floors = 5,
                        Units = new List<Unit>
                        {
                            new() { Type = UnitType.Apartment, Status = UnitStatus.ForSale, Price = 1200000, Area = 80, Rooms = 2, Bathrooms = 1, Floor = 1 }
                        }
                    }
                },
                Media = new List<ProjectMedia>
                {
                    new() { Type = MediaType.Image, MediaUrl = "https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?auto=format&fit=crop&w=800&q=80", IsThumbnail = true }
                }
            },
            new()
            {
                Name = "برج المنارة السكني",
                Status = ProjectStatus.ForSale,
                TotalArea = 2500,
                City = "الإسكندرية",
                Area = "سموحة",
                Address = "طريق الحرية، بجوار نادي سموحة",
                Latitude = 31.2224,
                Longitude = 29.9472,
                Description = "إطلالة بانورامية وتصاميم معمارية فريدة في أرقى مناطق الإسكندرية.",
                Buildings = new List<Building>
                {
                    new() 
                    { 
                        Name = "البرج الرئيسي", Floors = 12,
                        Units = new List<Unit>
                        {
                            new() { Type = UnitType.Apartment, Status = UnitStatus.ForSale, Price = 7500000, Area = 350, Rooms = 5, Bathrooms = 4, Floor = 12 },
                            new() { Type = UnitType.Apartment, Status = UnitStatus.ForSale, Price = 3500000, Area = 200, Rooms = 3, Bathrooms = 3, Floor = 8 }
                        }
                    }
                },
                Media = new List<ProjectMedia>
                {
                    new() { Type = MediaType.Image, MediaUrl = "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?auto=format&fit=crop&w=800&q=80", IsThumbnail = true }
                }
            },
            new()
            {
                Name = "قرية بلو فالي",
                Status = ProjectStatus.ForSale,
                TotalArea = 120000,
                City = "الساحل الشمالي",
                Area = "سيدي عبد الرحمن",
                Address = "الكيلو 130 طريق الإسكندرية مطروح",
                Description = "منتجع سياحي متكامل يتميز بشواطئه الرملية البيضاء ومياهه الفيروزية.",
                Buildings = new List<Building>
                {
                    new() 
                    { 
                        Name = "فيلات الصف الأول", Floors = 2,
                        Units = new List<Unit>
                        {
                            new() { Type = UnitType.Villa, Status = UnitStatus.ForSale, Price = 15000000, Area = 450, Rooms = 6, Bathrooms = 5, Floor = 0 }
                        }
                    }
                },
                Media = new List<ProjectMedia>
                {
                    new() { Type = MediaType.Image, MediaUrl = "https://images.unsplash.com/photo-1499793983690-e29da59ef1c2?auto=format&fit=crop&w=800&q=80", IsThumbnail = true }
                }
            },
            new()
            {
                Name = "كمبوند هايد بارك",
                Status = ProjectStatus.ForSale,
                TotalArea = 6000000,
                City = "القاهرة",
                Area = "القاهرة الجديدة",
                Address = "الدائري الأوسطي، التجمع الخامس",
                Description = "أكبر مساحة خضراء في التجمع الخامس بتصاميم عالمية.",
                Buildings = new List<Building>
                {
                    new() 
                    { 
                        Name = "منطقة الفيلات B", Floors = 2,
                        Units = new List<Unit>
                        {
                            new() { Type = UnitType.Villa, Status = UnitStatus.ForSale, Price = 25000000, Area = 600, Rooms = 7, Bathrooms = 6, Floor = 0 }
                        }
                    }
                },
                Media = new List<ProjectMedia>
                {
                    new() { Type = MediaType.Image, MediaUrl = "https://images.unsplash.com/photo-1518780664697-55e3ad937233?auto=format&fit=crop&w=800&q=80", IsThumbnail = true }
                }
            }
        };

        await context.Projects.AddRangeAsync(projects);
        await context.SaveChangesAsync();

        // 3. Link Features
        var projectFeatures = new List<ProjectFeature>();
        foreach(var project in projects)
        {
            projectFeatures.Add(new ProjectFeature { ProjectId = project.ProjectId, FeatureId = features[4].FeatureId }); // أمن
            projectFeatures.Add(new ProjectFeature { ProjectId = project.ProjectId, FeatureId = features[5].FeatureId }); // جراج
            projectFeatures.Add(new ProjectFeature { ProjectId = project.ProjectId, FeatureId = features[2].FeatureId }); // مساحات خضراء
        }
        
        await context.ProjectFeatures.AddRangeAsync(projectFeatures);
        await context.SaveChangesAsync();
    }
}
