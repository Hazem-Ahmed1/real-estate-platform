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
        await SeedLookupsAsync();
    }

    private async Task SeedLookupsAsync()
    {
        await SeedFeaturesAsync();
        await SeedInsuranceAsync();
    }

    private async Task SeedFeaturesAsync()
    {
        if (await context.Features.AnyAsync())
            return;

        var features = new List<Feature>
        {
            // Active Features
            new Feature { Name = "حمام سباحة", IsActive = true },
            new Feature { Name = "جراج مغلق", IsActive = true },
            new Feature { Name = "أمن وحراسة 24 ساعة", IsActive = true },
            new Feature { Name = "مصعد كهربائي", IsActive = true },
            new Feature { Name = "حديقة خاصة", IsActive = true },
            new Feature { Name = "نادي رياضي", IsActive = true },
            new Feature { Name = "مولد كهربائي", IsActive = true },
            new Feature { Name = "كاميرات مراقبة", IsActive = true },
            new Feature { Name = "تكييف مركزي", IsActive = true },
            new Feature { Name = "انترنت فايبر", IsActive = true },
            new Feature { Name = "مسجد", IsActive = true },
            new Feature { Name = "مدرسة داخل الكمبوند", IsActive = true },

            // Inactive Features (معطلة - لاختبار سيناريوهات التعطيل)
            new Feature { Name = "ملعب تنس", IsActive = false },
            new Feature { Name = "منطقة شواء", IsActive = false },
            new Feature { Name = "غرفة بواب", IsActive = false },
        };

        await context.Features.AddRangeAsync(features);
        await context.SaveChangesAsync();
    }

    private async Task SeedInsuranceAsync()
    {
        if (await context.Insurance.AnyAsync())
            return;

        var insurances = new List<Insurance>
        {
            // Active Insurance
            new Insurance { Name = "تأمين ضد الحريق", IsActive = true },
            new Insurance { Name = "تأمين ضد الفيضانات", IsActive = true },
            new Insurance { Name = "تأمين شامل على المبنى", IsActive = true },
            new Insurance { Name = "تأمين ضد السرقة", IsActive = true },
            new Insurance { Name = "تأمين المسؤولية المدنية", IsActive = true },
            new Insurance { Name = "تأمين ضد الزلازل", IsActive = true },
            new Insurance { Name = "تأمين على المحتويات", IsActive = true },
            new Insurance { Name = "تأمين الحوادث الشخصية", IsActive = true },
            new Insurance { Name = "تأمين ضد العيوب الإنشائية", IsActive = true },
            new Insurance { Name = "تأمين بضمان حكومي", IsActive = true },

            // Inactive Insurance (معطلة - لاختبار سيناريوهات التعطيل)
            new Insurance { Name = "تأمين مؤقت", IsActive = false },
            new Insurance { Name = "تأمين تجريبي", IsActive = false },
        };

        await context.Insurance.AddRangeAsync(insurances);
        await context.SaveChangesAsync();
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
}

