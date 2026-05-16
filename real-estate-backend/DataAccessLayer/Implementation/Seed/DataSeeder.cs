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
        await SeedRealEstateDataAsync();
    }

    private async Task SeedRealEstateDataAsync()
    {
        if (await context.Projects.AnyAsync())
            return;

        var allFeatures = await context.Features.Where(f => f.IsActive).ToListAsync();
        var allInsurances = await context.Insurance.Where(i => i.IsActive).ToListAsync();

        // --- 1. Project SALE (2 Buildings, Mixed Sale/Sold) ---
        var pSale = CreateProject("مشروع اللؤلؤة (للبيع)", ProjectStatus.Sale, 2, 5000);
        AddProjectMedia(pSale);
        
        var bSale1 = CreateBuilding("عمارة A1 - شقق", 1);
        pSale.Buildings.Add(bSale1);
        // Mix: 2 Sale, 1 Sold
        AddUnitsToBuilding(bSale1, 2, UnitStatus.Sale, allFeatures, allInsurances);
        var soldInSale = new Unit { Name = "شقة 105 (مباعة)", Rooms = 3, Salons = 1, Area = 140, Status = UnitStatus.Sold, Type = UnitType.Apartment, Price = 2000000, Street = "شارع اللؤلؤة" };
        AddMediaToUnit(soldInSale, "Sold_Sale_105");
        bSale1.Units.Add(soldInSale);

        var bSale2 = CreateBuilding("منطقة الفيلات A2", 2);
        pSale.Buildings.Add(bSale2);
        var villa = new Unit { Name = "فيلا الياسمين", Rooms = 6, Salons = 3, Area = 450, Status = UnitStatus.Sale, Type = UnitType.Villa, Price = 12000000, Street = "شارع الفيلات" };
        AddMediaToUnit(villa, "Villa_Sale_A2");
        bSale2.Units.Add(villa);
        
        // --- 2. Project RENT (1 Building, Mixed Rent/Rented) ---
        var pRent = CreateProject("برج النيل (للإيجار)", ProjectStatus.Rent, 1, 2000);
        AddProjectMedia(pRent);
        var bRent1 = CreateBuilding("برج Rent-1", 1);
        pRent.Buildings.Add(bRent1);
        // Mix: 2 Rent, 1 Rented
        AddUnitsToBuilding(bRent1, 2, UnitStatus.Rent, allFeatures, allInsurances);
        var rentedInRent = new Unit { Name = "استوديو 104 (مؤجر)", Rooms = 1, Salons = 1, Area = 70, Status = UnitStatus.Rented, Type = UnitType.Apartment, Price = 8000, Street = "شارع النيل" };
        AddMediaToUnit(rentedInRent, "Rented_Rent_104");
        bRent1.Units.Add(rentedInRent);

        // --- 3. Project SOLD (3 Buildings, ALL Sold) ---
        var pSold = CreateProject("كمبوند الياسمين (مباع بالكامل)", ProjectStatus.Sold, 3, 8000);
        AddProjectMedia(pSold);
        for (int i = 1; i <= 3; i++)
        {
            var b = CreateBuilding($"عمارة Sold-{i}", i);
            pSold.Buildings.Add(b);
            AddUnitsToBuilding(b, 2, UnitStatus.Sold, allFeatures, allInsurances); 
        }

        // --- 4. Project RENTED (4 Buildings, ALL Rented) ---
        var pRented = CreateProject("مجمع الواحة (مؤجر بالكامل)", ProjectStatus.Rented, 4, 10000);
        AddProjectMedia(pRented);
        for (int i = 1; i <= 4; i++)
        {
            var b = CreateBuilding($"عمارة Rented-{i}", i);
            pRented.Buildings.Add(b);
            AddUnitsToBuilding(b, 2, UnitStatus.Rented, allFeatures, allInsurances);
        }

        await context.Projects.AddRangeAsync(pSale, pRent, pSold, pRented);
        await context.SaveChangesAsync();
    }

    private Project CreateProject(string name, ProjectStatus status, int buildingsCount, double landArea)
    {
        return new Project
        {
            Name = name,
            Status = status,
            City = "القاهرة",
            Area = "التجمع",
            Address = "شارع التسعين",
            LandArea = landArea,
            BuildUpArea = landArea * 0.6,
            TotalBuildingArea = landArea * 0.8
        };
    }

    private Building CreateBuilding(string name, int index)
    {
        return new Building
        {
            Name = name,
            BuildingArea = 400 + (index * 50),
            MaxArea = 200 + (index * 20),
            FloorCount = 5 + index
        };
    }

    private void AddUnitsToBuilding(Building building, int count, UnitStatus status, List<Feature> allFeatures, List<Insurance> allInsurances)
    {
        for (int i = 1; i <= count; i++)
        {
            var unit = new Unit
            {
                Name = $"وحدة {i} - {building.Name}",
                Rooms = 2 + (i % 3),
                Salons = 1,
                Bathrooms = 2,
                Area = 100 + (i * 10),
                Floor = i,
                Price = 1000000 + (i * 100000),
                Type = UnitType.Apartment,
                Status = status,
                Street = "شارع رئيسي"
            };

            AddMediaToUnit(unit, $"{building.Name}_{i}");

            // 4. Add All Types of Nearby Facilities
            unit.NearbyFacilities.Add(new NearbyFacility { Name = "مسجد الإيمان", Type = FacilityType.Mosque, Distance = "200 متر", Area = 300 });
            unit.NearbyFacilities.Add(new NearbyFacility { Name = "مدرسة النهضة", Type = FacilityType.School, Distance = "1 كم", Area = 5000 });
            unit.NearbyFacilities.Add(new NearbyFacility { Name = "مستشفى الشفاء", Type = FacilityType.Hospital, Distance = "2 كم", Area = 8000 });
            unit.NearbyFacilities.Add(new NearbyFacility { Name = "مطعم المدينة", Type = FacilityType.Restaurant, Distance = "500 متر", Area = 150 });
            unit.NearbyFacilities.Add(new NearbyFacility { Name = "حديقة الزهور", Type = FacilityType.Park, Distance = "300 متر", Area = 10000 });
            unit.NearbyFacilities.Add(new NearbyFacility { Name = "البنك الأهلي", Type = FacilityType.Bank, Distance = "800 متر", Area = 400 });
            unit.NearbyFacilities.Add(new NearbyFacility { Name = "صيدلية العزبي", Type = FacilityType.Pharmacy, Distance = "100 متر", Area = 50 });
            unit.NearbyFacilities.Add(new NearbyFacility { Name = "سوبر ماركت مترو", Type = FacilityType.SuperMarket, Distance = "400 متر", Area = 600 });
            unit.NearbyFacilities.Add(new NearbyFacility { Name = "نادي الجزيرة", Type = FacilityType.Club, Distance = "3 كم", Area = 20000 });

            building.Units.Add(unit);

            // 5. Add Features & Insurance to Unit
            foreach (var f in allFeatures.Take(3)) unit.UnitFeatures.Add(new UnitFeature { Feature = f });
            foreach (var ins in allInsurances.Take(2)) unit.UnitInsurance.Add(new UnitInsurance { Insurance = ins });
        }
    }

    private void AddMediaToUnit(Unit unit, string identifier)
    {
        // 1. Add 6 Images (1 Thumbnail + 5 Others)
        for (int j = 1; j <= 6; j++)
        {
            unit.Media.Add(new UnitMedia 
            { 
                MediaUrl = $"https://images.pexels.com/photos/1571460/pexels-photo-1571460.jpeg?u={identifier}_{j}", 
                PublicId = $"u_{identifier}_img_{j}", 
                Type = MediaType.Image, 
                IsThumbnail = j == 1 
            });
        }

        // 2. Add 5 Design Images
        for (int d = 1; d <= 5; d++)
        {
            unit.Media.Add(new UnitMedia 
            { 
                MediaUrl = $"https://images.pexels.com/photos/1571468/pexels-photo-1571468.jpeg?d={identifier}_{d}", 
                PublicId = $"u_{identifier}_design_{d}", 
                Type = MediaType.Design, 
                IsThumbnail = false 
            });
        }

        // 3. Add 1 Video & 1 Panorama
        unit.Media.Add(new UnitMedia { 
            MediaUrl = "https://res.cloudinary.com/demo/video/upload/v1625126868/sample_video.mp4", 
            PublicId = $"u_{identifier}_video", 
            Type = MediaType.Video,
            IsThumbnail = false
        });
        unit.Media.Add(new UnitMedia { 
            MediaUrl = "https://res.cloudinary.com/demo/image/upload/v1532512686/sample_panorama.jpg", 
            PublicId = $"u_{identifier}_pano", 
            Type = MediaType.Panorama360,
            IsThumbnail = false
        });
    }

    private void AddProjectMedia(Project project)
    {
        // 1. Add 6 Images (1 Thumbnail + 5 Others)
        for (int i = 1; i <= 6; i++)
        {
            project.Media.Add(new ProjectMedia { 
                MediaUrl = $"https://images.pexels.com/photos/302769/pexels-photo-302769.jpeg?p={project.Name}_{i}", 
                PublicId = $"p_{project.Name}_img_{i}", 
                Type = MediaType.Image, 
                IsThumbnail = i == 1 
            });
        }

        // 2. Add 1 Video & 1 Panorama
        project.Media.Add(new ProjectMedia { 
            MediaUrl = "https://res.cloudinary.com/demo/video/upload/v1625126868/sample_video.mp4", 
            PublicId = $"p_{project.Name}_video", 
            Type = MediaType.Video,
            IsThumbnail = false
        });
        project.Media.Add(new ProjectMedia { 
            MediaUrl = "https://res.cloudinary.com/demo/image/upload/v1532512686/sample_panorama.jpg", 
            PublicId = $"p_{project.Name}_pano", 
            Type = MediaType.Panorama360,
            IsThumbnail = false
        });
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

