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
    private int streetCountSeed = 0;

    public async Task SeedAsync()
    {
        await SeedIdentityAsync();
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

