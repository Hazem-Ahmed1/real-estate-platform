using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.CommunicationModule;
using DataAccessLayer.Entities.LookupModule;

namespace BusinessLogicLayer.Implementation;

public class DashboardService(IUnitOfWork unitOfWork) : IDashboardService
{
    public async Task<object> GetStatsAsync()
    {
        // 1. High-Level Summary Metrics (Cards)
        var totalProjects = await unitOfWork.Repository<Project>().CountAsync(p => true);
        var totalBuildings = await unitOfWork.Repository<Building>().CountAsync(b => true);
        var totalUnits = await unitOfWork.Repository<Unit>().CountAsync(u => true);
        var totalMessages = await unitOfWork.Repository<Message>().CountAsync(m => true);
        var totalBlogs = await unitOfWork.Repository<BlogPost>().CountAsync(b => true);
        var totalFeatures = await unitOfWork.Repository<Feature>().CountAsync(f => true);
        var totalInsurance = await unitOfWork.Repository<Insurance>().CountAsync(i => true);

        // Fetching clean sets for complex LINQ calculations (Efficient tracking off)
        var allUnits = (await unitOfWork.Repository<Unit>().GetAllAsync(asNoTracking: true)).ToList();
        var allProjects = (await unitOfWork.Repository<Project>().GetAllAsync(asNoTracking: true)).ToList();

        // 2. Financial Metrics (Revenue & Value Cards)
        var totalInventoryValue = allUnits.Where(u => u.Status == UnitStatus.Sale).Sum(u => u.Price);
        var totalSalesRevenue = allUnits.Where(u => u.Status == UnitStatus.Sold).Sum(u => u.Price);
        var totalRentMonthlyRevenue = allUnits.Where(u => u.Status == UnitStatus.Rented).Sum(u => u.Price);
        var avgPricePerUnit = allUnits.Select(u => u.Price).DefaultIfEmpty(0).Average();

        // 3. Distribution Metrics (Pie / Doughnut Charts)
        var unitStatusDistribution = allUnits
            .GroupBy(u => u.Status)
            .Select(g => new { name = g.Key.ToString(), count = g.Count() })
            .ToList();

        var unitTypeDistribution = allUnits
            .GroupBy(u => u.Type)
            .Select(g => new { name = g.Key.ToString(), count = g.Count() })
            .ToList();

        var projectStatusDistribution = allProjects
            .GroupBy(p => p.Status)
            .Select(g => new { name = g.Key.ToString(), count = g.Count() })
            .ToList();

        // 4. Advanced Analysis (Bar Charts)
        var avgPriceByUnitType = allUnits
            .GroupBy(u => u.Type)
            .Select(g => new { 
                type = g.Key.ToString(), 
                averagePrice = Math.Round(g.Select(u => u.Price).DefaultIfEmpty(0).Average(), 2) 
            })
            .ToList();

        var projectsByCity = allProjects
            .GroupBy(p => string.IsNullOrEmpty(p.City) ? "غير محدد" : p.City)
            .Select(g => new { 
                city = g.Key, 
                count = g.Count() 
            })
            .ToList();

        // 5. Performance Progress Lists (KPIs)
        var topPerformingProjects = allProjects
            .Select(p => {
                var total = p.TransactedUnitsCount + p.AvailableUnitsCount;
                var sold = p.TransactedUnitsCount;
                return new {
                    projectId = p.ProjectId,
                    name = p.Name,
                    progressPercentage = total > 0 ? Math.Round((double)sold / total * 100, 2) : 0,
                    totalUnits = total,
                    transactedUnits = sold
                };
            })
            .OrderByDescending(x => x.progressPercentage)
            .Take(5)
            .ToList();

        // 6. Recent Activities Lists
        var recentUnits = allUnits
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .Select(u => new {
                unitId = u.UnitId,
                name = u.Name,
                price = u.Price,
                status = u.Status.ToString(),
                createdAt = u.CreatedAt
            })
            .ToList();

        var recentProjects = allProjects
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new {
                projectId = p.ProjectId,
                name = p.Name,
                status = p.Status.ToString(),
                createdAt = p.CreatedAt
            })
            .ToList();

        var recentMessages = (await unitOfWork.Repository<Message>().GetAllAsync(asNoTracking: true))
            .OrderByDescending(m => m.CreatedAt)
            .Take(5)
            .Select(m => new {
                messageId = m.MessageId,
                senderName = m.FullName,
                email = m.Email,
                subject = m.Subject,
                createdAt = m.CreatedAt
            })
            .ToList();

        return new
        {
            cards = new
            {
                totalProjects,
                totalBuildings,
                totalUnits,
                totalBlogs,
                totalMessages,
                totalFeatures,
                totalInsurance,
                financials = new
                {
                    totalInventoryValue,
                    totalSalesRevenue,
                    totalRentMonthlyRevenue,
                    avgPrice = Math.Round(avgPricePerUnit, 2)
                }
            },
            pieCharts = new
            {
                unitStatuses = unitStatusDistribution,
                unitTypes = unitTypeDistribution,
                projectStatuses = projectStatusDistribution
            },
            barCharts = new
            {
                averagePriceByType = avgPriceByUnitType,
                projectsDistributionByCity = projectsByCity
            },
            kpis = new
            {
                topProjects = topPerformingProjects,
                occupancyRate = totalUnits > 0 ? Math.Round((double)allUnits.Count(u => u.Status == UnitStatus.Sold || u.Status == UnitStatus.Rented) / totalUnits * 100, 2) : 0
            },
            recentActivities = new
            {
                units = recentUnits,
                projects = recentProjects,
                messages = recentMessages
            }
        };
    }

    public async Task<object> GetChartAsync()
    {
        var allUnits = (await unitOfWork.Repository<Unit>().GetAllAsync(asNoTracking: true)).ToList();
        
        var yearToUse = allUnits.Any() ? allUnits.Max(u => u.CreatedAt.Year) : DateTime.UtcNow.Year;
        var monthlyData = allUnits
            .Where(u => u.CreatedAt.Year == yearToUse)
            .GroupBy(u => u.CreatedAt.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .ToList();

        var months = new[] { "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو", "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" };
        var counts = new int[12];
        foreach (var m in monthlyData)
        {
            if (m.Month >= 1 && m.Month <= 12)
            {
                counts[m.Month - 1] = m.Count;
            }
        }

        return new
        {
            labels = months,
            datasets = new[]
            {
                new { 
                    label = "العقارات المضافة حديثاً", 
                    data = counts,
                    backgroundColor = "rgba(79, 70, 229, 0.2)",
                    borderColor = "rgb(79, 70, 229)",
                    borderWidth = 2
                }
            }
        };
    }

    public async Task<object> GetPublicStatsAsync()
    {
        var totalProjects = await unitOfWork.Repository<Project>().CountAsync(p => true);
        var totalBuildings = await unitOfWork.Repository<Building>().CountAsync(b => true);
        var totalUnits = await unitOfWork.Repository<Unit>().CountAsync(u => true);

        return new
        {
            totalProjects,
            totalBuildings,
            totalUnits
        };
    }
}
