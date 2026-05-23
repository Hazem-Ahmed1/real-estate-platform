using System.Diagnostics;
using System.Globalization;
using DataAccessLayer.Contracts;
using DataAccessLayer.Data;
using DataAccessLayer.Entities.AIModule;
using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.CommunicationModule;
using DataAccessLayer.Entities.Identity;
using DataAccessLayer.Entities.LookupModule;
using DataAccessLayer.Entities.ProjectModule;
using DataAccessLayer.Entities.UnitModule;
using DataAccessLayer.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace APILayer.Seed;

public class DataSeeder(
    RealEstateDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration
) : IDataSeeder
{
    private const string AdminRoleName = "Admin";
    private readonly Random random = new(42);
    private readonly Stopwatch stopwatch = new();
    private int progressCurrent;
    private int progressTotal;

    public async Task SeedAsync()
    {
        InitializeProgress();
        await SeedIdentityAsync();
        ReportProgress("تم تجهيز حسابات الإدارة");
        await SeedLookupsAsync();
        ReportProgress("تم تجهيز القوائم الأساسية");
        await SeedProjectsAsync();
        ReportProgress("تم تجهيز المشروعات والوحدات");
        await SeedBlogsAsync();
        ReportProgress("تم تجهيز المدونة");
        await SeedMessagesAsync();
        ReportProgress("تم تجهيز الرسائل");
        await SeedAiAsync();
        ReportProgress("تم تجهيز البيانات المساندة");
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

    private async Task SeedLookupsAsync()
    {
        if (!await context.Features.AnyAsync())
        {
            context.Features.AddRange(
                new Feature { Name = "أمن وحراسة" },
                new Feature { Name = "حمام سباحة" },
                new Feature { Name = "جيم" },
                new Feature { Name = "موقف سيارات" },
                new Feature { Name = "حدائق" },
                new Feature { Name = "مصعد" },
                new Feature { Name = "شرفة" },
                new Feature { Name = "تكييف مركزي" },
                new Feature { Name = "نظام ذكي" },
                new Feature { Name = "إطلالة بحرية" },
                new Feature { Name = "نادي اجتماعي" },
                new Feature { Name = "منطقة ألعاب" }
            );
        }

        if (!await context.Insurance.AnyAsync())
        {
            context.Insurance.AddRange(
                new Insurance { Name = "تأمين ضد الحريق", Duration = 12 },
                new Insurance { Name = "تأمين شامل للمباني", Duration = 24 },
                new Insurance { Name = "تأمين على المحتويات", Duration = 12 },
                new Insurance { Name = "تأمين مسؤولية مدنية", Duration = 18 },
                new Insurance { Name = "تأمين سرقة", Duration = 12 },
                new Insurance { Name = "تأمين صيانة", Duration = 6 },
                new Insurance { Name = "تأمين مصاعد", Duration = 24 },
                new Insurance { Name = "تأمين زلازل", Duration = 36 },
                new Insurance { Name = "تأمين مخاطر هندسية", Duration = 18 },
                new Insurance { Name = "تأمين مسكن" , Duration = 12 },
                new Insurance { Name = "تأمين حدائق" , Duration = 12 },
                new Insurance { Name = "تأمين مرافق" , Duration = 24 }
            );
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedProjectsAsync()
    {
        if (await context.Projects.AnyAsync())
        {
            return;
        }

        var features = await context.Features.AsNoTracking().ToListAsync();
        var insurances = await context.Insurance.AsNoTracking().ToListAsync();

        var projectSeeds = BuildProjectSeeds();
        var projects = new List<Project>();
        var buildings = new List<Building>();

        foreach (var seed in projectSeeds)
        {
            var project = new Project
            {
                Name = seed.Name,
                City = seed.City,
                Region = seed.Region,
                Address = seed.Address,
                Latitude = seed.Latitude,
                Longitude = seed.Longitude,
                LandArea = seed.LandArea,
                BuildUpArea = seed.BuildUpArea,
                Status = seed.ProjectStatus,
                AvailableUnitsCount = 0,
                TransactedUnitsCount = 0
            };

            var featureNames = PickMany(features.Select(f => f.Name).ToList(), 4);
            var insuranceNames = PickMany(insurances.Select(i => i.Name).ToList(), 3);
            AddProjectLookups(project, features, insurances, featureNames, insuranceNames);

            projects.Add(project);

            var building = new Building
            {
                Name = $"{seed.BuildingPrefix} - برج {seed.BuildingIndex}",
                Project = project,
                MaxArea = seed.MaxArea,
                BuildingArea = seed.BuildingArea,
                Type = seed.BuildingType,
                FloorCount = seed.FloorCount
            };

            buildings.Add(building);
        }

        context.Projects.AddRange(projects);
        await context.SaveChangesAsync();
        ReportProgress("تم حفظ المشروعات");

        foreach (var building in buildings)
        {
            building.ProjectId = building.Project.ProjectId;
        }

        context.Buildings.AddRange(buildings);
        await context.SaveChangesAsync();
        ReportProgress("تم حفظ المباني");

        var units = new List<Unit>();
        var unitTypes = new[] { UnitType.Apartment, UnitType.Duplex, UnitType.Office };

        for (var index = 0; index < buildings.Count; index++)
        {
            var building = buildings[index];
            var project = building.Project;
            var baseStatus = GetUnitStatusForProject(project.Status);
            var basePrice = 1500000m + (index * 250000m);

            for (var i = 0; i < 2; i++)
            {
                var unitName = $"وحدة {index + 1}-{i + 1} في {building.Name}";
                var unit = await CreateUnitAsync(
                    name: unitName,
                    building: building,
                    status: baseStatus,
                    type: unitTypes[(index + i) % unitTypes.Length],
                    rooms: 2 + ((index + i) % 3),
                    salons: 1,
                    area: 120 + ((index + i) * 10),
                    baths: 1 + ((index + i) % 2),
                    floor: 1 + (i % 3),
                    price: basePrice + (i * 150000m),
                    streetCount: 1 + ((index + i) % 3),
                    latitude: project.Latitude + 0.001 * (i + 1),
                    longitude: project.Longitude + 0.001 * (i + 1),
                    featureNames: PickMany(features.Select(f => f.Name).ToList(), 3),
                    insuranceNames: PickMany(insurances.Select(i => i.Name).ToList(), 2)
                );

                units.Add(unit);
                ReportProgress($"تم تجهيز بيانات وحدة {unit.Name}");
            }
        }

        context.Units.AddRange(units);
        await context.SaveChangesAsync();
        ReportProgress("تم حفظ الوحدات");

        foreach (var project in projects)
        {
            var projectBuildings = buildings.Where(b => b.ProjectId == project.ProjectId).ToList();
            UpdateProjectStatistics(project, projectBuildings, units);
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedBlogsAsync()
    {
        if (await context.BlogPosts.AnyAsync())
        {
            return;
        }

        var blogTitles = new[]
        {
            "دليل شراء شقق في القاهرة الجديدة",
            "أفضل مناطق الاستثمار العقاري في القاهرة ٢٠٢٦",
            "كيف تختار وحدة قريبة من الخدمات",
            "اتجاهات الأسعار في القاهرة الجديدة",
            "مشروعات جديدة في العاصمة الإدارية",
            "نصائح للتشطيب والاستلام",
            "مقارنة بين الشقق والدوبلكس",
            "العيش بالقرب من المترو والمواصلات في القاهرة",
            "أهم المميزات في المجمعات السكنية",
            "خطط دفع مرنة للمشروعات الجديدة",
            "خطوات حجز وحدة بكل سهولة",
            "توقعات السوق العقاري في القاهرة الكبرى"
        };

        var blogs = new List<BlogPost>();
        for (var i = 0; i < blogTitles.Length; i++)
        {
            blogs.Add(new BlogPost
            {
                Title = blogTitles[i],
                Description = "مقال سريع يقدم لمحة عربية مختصرة عن آخر تطورات السوق وفرص الشراء المناسبة.",
                PublishDate = DateTime.UtcNow.AddDays(-(i + 1)),
                Images = new List<BlogImage>()
            });
        }

        context.BlogPosts.AddRange(blogs);
        await context.SaveChangesAsync();
        ReportProgress("تم حفظ تدوينات المدونة");
    }

    private async Task SeedMessagesAsync()
    {
        if (await context.Messages.AnyAsync())
        {
            return;
        }

        var messages = new List<Message>
        {
            new Message
            {
                Type = MessageType.GeneralContact,
                FullName = "محمد علي",
                Email = "mohamed.ali@example.com",
                Phone = "+20 100 111 0001",
                Subject = "طلب كتيب المشروع",
                MessageBody = "يرجى إرسال أحدث كتيب للمشروعات المتاحة في القاهرة الجديدة."
            },
            new Message
            {
                Type = MessageType.UnitEnquiry,
                FullName = "سارة محمود",
                Email = "sara.mahmoud@example.com",
                Phone = "+20 100 111 0002",
                Subject = "استفسار عن وحدة سكنية",
                MessageBody = "هل توجد وحدات متاحة للبيع مع خطط دفع مرنة؟"
            },
            new Message
            {
                Type = MessageType.GeneralContact,
                FullName = "أحمد حلمي",
                Email = "ahmed.helmy@example.com",
                Phone = "+20 100 111 0003",
                Subject = "زيارة ميدانية",
                MessageBody = "أرغب في تحديد موعد لزيارة أحد المشروعات في القاهرة الجديدة."
            },
            new Message
            {
                Type = MessageType.UnitEnquiry,
                FullName = "نور هاني",
                Email = "noor.hany@example.com",
                Phone = "+20 100 111 0004",
                Subject = "وحدة للإيجار",
                MessageBody = "هل هناك وحدات للإيجار في منطقة الشيخ زايد؟"
            },
            new Message
            {
                Type = MessageType.GeneralContact,
                FullName = "خالد يوسف",
                Email = "khaled.youssef@example.com",
                Phone = "+20 100 111 0005",
                Subject = "معلومات عن التأمين",
                MessageBody = "ما هي خيارات التأمين المتاحة للوحدات الجديدة؟"
            },
            new Message
            {
                Type = MessageType.UnitEnquiry,
                FullName = "مها حسن",
                Email = "maha.hassan@example.com",
                Phone = "+20 100 111 0006",
                Subject = "وحدة بحديقة",
                MessageBody = "أبحث عن وحدة بحديقة ومساحة كبيرة في القاهرة الجديدة."
            },
            new Message
            {
                Type = MessageType.GeneralContact,
                FullName = "أسماء سامي",
                Email = "asmaa.sami@example.com",
                Phone = "+20 100 111 0007",
                Subject = "سؤال عن خدمات الصيانة",
                MessageBody = "هل يوجد برنامج صيانة سنوي للمرافق؟"
            },
            new Message
            {
                Type = MessageType.UnitEnquiry,
                FullName = "يوسف نادر",
                Email = "youssef.nader@example.com",
                Phone = "+20 100 111 0008",
                Subject = "وحدة قريبة من الخدمات",
                MessageBody = "أرغب في وحدة قريبة من الخدمات والمراكز التجارية."
            },
            new Message
            {
                Type = MessageType.GeneralContact,
                FullName = "مريم فؤاد",
                Email = "mariam.fouad@example.com",
                Phone = "+20 100 111 0009",
                Subject = "تفاصيل الأسعار",
                MessageBody = "يرجى إرسال تفاصيل الأسعار وخطط الدفع."
            },
            new Message
            {
                Type = MessageType.UnitEnquiry,
                FullName = "طارق سعد",
                Email = "tarek.saad@example.com",
                Phone = "+20 100 111 0010",
                Subject = "وحدة مكتبية",
                MessageBody = "هل تتوفر وحدات مكتبية للبيع في المشروعات الجديدة؟"
            },
            new Message
            {
                Type = MessageType.GeneralContact,
                FullName = "لينا إبراهيم",
                Email = "lina.ibrahim@example.com",
                Phone = "+20 100 111 0011",
                Subject = "مشروعات في القاهرة",
                MessageBody = "أبحث عن مشروعات جديدة في القاهرة الجديدة أو العاصمة الإدارية."
            },
            new Message
            {
                Type = MessageType.UnitEnquiry,
                FullName = "زياد شريف",
                Email = "ziyad.sherif@example.com",
                Phone = "+20 100 111 0012",
                Subject = "حجز مبدئي",
                MessageBody = "هل يمكنني الحجز المبدئي أونلاين؟"
            }
        };

        context.Messages.AddRange(messages);
        await context.SaveChangesAsync();
        ReportProgress("تم حفظ الرسائل");
    }

    private async Task SeedAiAsync()
    {
        if (await context.KnowledgeBase.AnyAsync() || await context.ChatHistory.AnyAsync())
        {
            return;
        }

        context.KnowledgeBase.AddRange(
            new KnowledgeBaseEntry
            {
                EntityType = KnowledgeEntityType.Project,
                EntityId = null,
                Content = "مشروع لؤلؤة النيل يقدم وحدات سكنية للبيع بمرافق حديثة في القاهرة الجديدة.",
                VectorData = "[]"
            },
            new KnowledgeBaseEntry
            {
                EntityType = KnowledgeEntityType.Project,
                EntityId = null,
                Content = "مشروع باي البحر بالإسكندرية يوفر وحدات مطلة على البحر وخدمات متكاملة.",
                VectorData = "[]"
            }
        );

        context.ChatHistory.Add(
            new ChatHistory
            {
                SessionId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
                UserMessage = "هل توجد وحدات مطلة على البحر؟",
                AiResponse = "يوجد وحدات مطلة على البحر في مشروعات الإسكندرية مع خطط سداد مرنة."
            }
        );

        await context.SaveChangesAsync();
        ReportProgress("تم حفظ بيانات الذكاء الاصطناعي");
    }

    private List<ProjectSeed> BuildProjectSeeds()
    {
        return new List<ProjectSeed>
        {
            new("لؤلؤة النيل", "القاهرة", "القاهرة الجديدة", "شارع التسعين الشمالي", 30.0131, 31.4913, 48000, 28000, ProjectStatus.Sale, 9500, 320, 12, BuildingType.Sale, "لؤلؤة النيل", 1),
            new("بوابة التجمع", "القاهرة", "التجمع الخامس", "شارع التسعين الجنوبي", 30.0205, 31.4750, 42000, 24000, ProjectStatus.Sale, 8500, 300, 10, BuildingType.Sale, "بوابة التجمع", 2),
            new("فلل زايد", "القاهرة", "الشيخ زايد", "محور 26 يوليو", 30.0459, 30.9718, 40000, 22000, ProjectStatus.Sale, 7800, 280, 9, BuildingType.Sale, "فلل زايد", 3),
            new("سكاي أكتوبر", "القاهرة", "مدينة ٦ أكتوبر", "طريق الواحات", 29.9773, 30.9541, 36000, 20000, ProjectStatus.Sale, 7200, 260, 8, BuildingType.Sale, "سكاي أكتوبر", 4),
            new("جرين لاند", "القاهرة", "مدينتي", "الطريق الدائري الأوسط", 30.0842, 31.6373, 35000, 19000, ProjectStatus.Sold, 7000, 250, 8, BuildingType.Sale, "جرين لاند", 5),
            new("وادي النخيل", "القاهرة", "الرحاب", "طريق السويس", 30.0736, 31.6903, 34000, 18000, ProjectStatus.Sold, 6800, 240, 7, BuildingType.Sale, "وادي النخيل", 6),
            new("ريف العاصمة", "القاهرة", "العاصمة الإدارية", "حي المال والأعمال", 30.0300, 31.7187, 33000, 17000, ProjectStatus.Rent, 6500, 230, 7, BuildingType.Rent, "ريف العاصمة", 7),
            new("لؤلؤة الشروق", "القاهرة", "الشروق", "طريق القاهرة السويس", 30.1477, 31.6232, 32000, 16500, ProjectStatus.Rent, 6300, 220, 7, BuildingType.Rent, "لؤلؤة الشروق", 8),
            new("قصر بدر", "القاهرة", "مدينة بدر", "طريق السويس", 30.1368, 31.7101, 31000, 16000, ProjectStatus.Rent, 6100, 210, 6, BuildingType.Rent, "قصر بدر", 9),
            new("نيل المعادي", "القاهرة", "المعادي", "شارع النصر", 29.9624, 31.2651, 30000, 15500, ProjectStatus.Rented, 6000, 200, 6, BuildingType.Rent, "نيل المعادي", 10),
            new("واحة مصر الجديدة", "القاهرة", "مصر الجديدة", "شارع الثورة", 30.0917, 31.3223, 29000, 15000, ProjectStatus.Rented, 5800, 190, 6, BuildingType.Rent, "واحة مصر الجديدة", 11),
            new("مارينا التجمع", "القاهرة", "التجمع الأول", "شارع التسعين الشرقي", 30.0555, 31.4488, 31000, 17000, ProjectStatus.Sale, 6400, 220, 7, BuildingType.Sale, "مارينا التجمع", 12)
        };
    }

    private UnitStatus GetUnitStatusForProject(ProjectStatus status)
    {
        return status switch
        {
            ProjectStatus.Sold => UnitStatus.Sold,
            ProjectStatus.Rented => UnitStatus.Rented,
            ProjectStatus.Rent => UnitStatus.Rent,
            _ => UnitStatus.Sale
        };
    }

    private void AddProjectLookups(Project project, List<Feature> features, List<Insurance> insurances, IEnumerable<string> featureNames, IEnumerable<string> insuranceNames)
    {
        foreach (var name in featureNames.Distinct(StringComparer.Ordinal))
        {
            var feature = features.FirstOrDefault(f => f.Name == name);
            if (feature != null)
            {
                project.ProjectFeatures.Add(new ProjectFeature { FeatureId = feature.FeatureId });
            }
        }

        foreach (var name in insuranceNames.Distinct(StringComparer.Ordinal))
        {
            var insurance = insurances.FirstOrDefault(i => i.Name == name);
            if (insurance != null)
            {
                project.ProjectInsurance.Add(new ProjectInsurance { InsuranceId = insurance.InsuranceId });
            }
        }
    }

    private async Task<Unit> CreateUnitAsync(
        string name,
        Building building,
        UnitStatus status,
        UnitType type,
        int rooms,
        int salons,
        double area,
        int baths,
        int floor,
        decimal price,
        int streetCount,
        double latitude,
        double longitude,
        IEnumerable<string> featureNames,
        IEnumerable<string> insuranceNames)
    {
        var unit = new Unit
        {
            Name = name,
            BuildingId = building.BuildingId,
            Rooms = rooms,
            Salons = salons,
            Area = area,
            Bathrooms = baths,
            Floor = floor,
            Price = price,
            Type = type,
            Status = status,
            StreetCount = streetCount,
            City = building.Project.City,
            Region = building.Project.Region,
            Address = building.Project.Address,
            Latitude = latitude,
            Longitude = longitude
        };

        var features = await context.Features.AsNoTracking().ToListAsync();
        var insurances = await context.Insurance.AsNoTracking().ToListAsync();

        foreach (var nameValue in featureNames.Distinct(StringComparer.Ordinal))
        {
            var feature = features.FirstOrDefault(f => f.Name == nameValue);
            if (feature != null)
            {
                unit.UnitFeatures.Add(new UnitFeature { FeatureId = feature.FeatureId });
            }
        }

        foreach (var nameValue in insuranceNames.Distinct(StringComparer.Ordinal))
        {
            var insurance = insurances.FirstOrDefault(i => i.Name == nameValue);
            if (insurance != null)
            {
                unit.UnitInsurance.Add(new UnitInsurance { InsuranceId = insurance.InsuranceId });
            }
        }

        var nearbyFacilities = new[]
        {
            new NearbyFacility
            {
                Type = FacilityType.School,
                Name = "مدرسة دولية",
                Distance = "850 م",
                Area = 1200,
                Latitude = latitude + 0.002,
                Longitude = longitude + 0.002
            },
            new NearbyFacility
            {
                Type = FacilityType.Park,
                Name = "حديقة مركزية",
                Distance = "500 م",
                Area = 3200,
                Latitude = latitude - 0.001,
                Longitude = longitude - 0.001
            }
        };

        foreach (var facility in nearbyFacilities)
        {
            unit.NearbyFacilities.Add(facility);
        }

        return unit;
    }

    private List<string> PickMany(IReadOnlyList<string> source, int count)
    {
        var results = new HashSet<string>(StringComparer.Ordinal);
        if (source.Count == 0)
        {
            return new List<string>();
        }

        var attempts = 0;
        while (results.Count < count && attempts < source.Count * 3)
        {
            results.Add(source[random.Next(source.Count)]);
            attempts++;
        }

        return results.ToList();
    }

    private void UpdateProjectStatistics(Project project, IEnumerable<Building> buildings, IEnumerable<Unit> allUnits)
    {
        var projectBuildings = buildings.ToList();
        var projectUnits = allUnits.Where(u => projectBuildings.Any(b => b.BuildingId == u.BuildingId)).ToList();

        foreach (var building in projectBuildings)
        {
            var buildingUnits = projectUnits.Where(u => u.BuildingId == building.BuildingId).ToList();
            building.Status = DeriveBuildingStatus(building.Type, buildingUnits);
            if (buildingUnits.Any())
            {
                building.FloorCount = buildingUnits.Max(u => u.Floor);
            }
        }

        project.Status = DeriveProjectStatus(projectUnits, project.Status);
        project.AvailableUnitsCount = projectUnits.Count(u => u.Status == UnitStatus.Sale || u.Status == UnitStatus.Rent);
        project.TransactedUnitsCount = projectUnits.Count(u => u.Status == UnitStatus.Sold || u.Status == UnitStatus.Rented);
        project.TotalBuildingArea = projectBuildings.Sum(b => b.BuildingArea);
    }

    private static UnitStatus? DeriveBuildingStatus(BuildingType type, IEnumerable<Unit> units)
    {
        var list = units.ToList();
        if (!list.Any())
        {
            return null;
        }

        return type == BuildingType.Sale
            ? list.All(u => u.Status == UnitStatus.Sold) ? UnitStatus.Sold : UnitStatus.Sale
            : list.All(u => u.Status == UnitStatus.Rented) ? UnitStatus.Rented : UnitStatus.Rent;
    }

    private static ProjectStatus DeriveProjectStatus(IEnumerable<Unit> units, ProjectStatus currentStatus)
    {
        var list = units.ToList();
        if (!list.Any())
        {
            return currentStatus;
        }

        var saleUnits = list.Where(u => u.Status == UnitStatus.Sale || u.Status == UnitStatus.Sold).ToList();
        var rentUnits = list.Where(u => u.Status == UnitStatus.Rent || u.Status == UnitStatus.Rented).ToList();

        if (saleUnits.Any() && !rentUnits.Any())
        {
            return saleUnits.All(u => u.Status == UnitStatus.Sold) ? ProjectStatus.Sold : ProjectStatus.Sale;
        }

        if (rentUnits.Any() && !saleUnits.Any())
        {
            return rentUnits.All(u => u.Status == UnitStatus.Rented) ? ProjectStatus.Rented : ProjectStatus.Rent;
        }

        var isSalePath = currentStatus == ProjectStatus.Sale || currentStatus == ProjectStatus.Sold;
        return isSalePath
            ? saleUnits.All(u => u.Status == UnitStatus.Sold) ? ProjectStatus.Sold : ProjectStatus.Sale
            : rentUnits.All(u => u.Status == UnitStatus.Rented) ? ProjectStatus.Rented : ProjectStatus.Rent;
    }

    private sealed record ProjectSeed(
        string Name,
        string City,
        string Region,
        string Address,
        double Latitude,
        double Longitude,
        double LandArea,
        double BuildUpArea,
        ProjectStatus ProjectStatus,
        double BuildingArea,
        double MaxArea,
        int FloorCount,
        BuildingType BuildingType,
        string BuildingPrefix,
        int BuildingIndex
    );

    private void InitializeProgress()
    {
        progressTotal = EstimateTotalSteps();
        progressCurrent = 0;
        stopwatch.Restart();
        Console.WriteLine($"Seeding started at {DateTime.Now:HH:mm:ss}");
    }

    private int EstimateTotalSteps()
    {
        var projectCount = BuildProjectSeeds().Count;
        var unitCount = projectCount * 2;
        var fixedSteps = 6;
        var projectSteps = projectCount;
        var unitSteps = unitCount;
        var blogSteps = 1;
        var messageSteps = 1;
        var aiSteps = 1;
        return fixedSteps + projectSteps + unitSteps + blogSteps + messageSteps + aiSteps;
    }

    private void ReportProgress(string message)
    {
        progressCurrent = Math.Min(progressCurrent + 1, progressTotal);
        var percent = progressTotal == 0 ? 100 : (int)Math.Round(progressCurrent * 100.0 / progressTotal);
        Console.WriteLine($"[Seed {percent}%] {message} | elapsed {stopwatch.Elapsed:hh\\:mm\\:ss}");
    }
}
