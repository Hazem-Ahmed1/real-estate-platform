using DataAccessLayer.Entities.BlogModule;
using DataAccessLayer.Entities.CommunicationModule;
using DataAccessLayer.Entities.LookupModule;

namespace BusinessLogicLayer.Implementation;

public class DashboardService(IUnitOfWork unitOfWork) : IDashboardService
{
    public async Task<object> GetStatsAsync()
    {
        // 1. High-Level Performance Counts (No table scans)
        var totalProjects = await unitOfWork.Repository<Project>().CountAsync(p => true);
        var totalBuildings = await unitOfWork.Repository<Building>().CountAsync(b => true);


        var totalUnits = await unitOfWork.Repository<Unit>().CountAsync(u => true);
        var totalMessages = await unitOfWork.Repository<Message>().CountAsync(m => true);
        var totalBlogs = await unitOfWork.Repository<BlogPost>().CountAsync(b => true);
        var totalFeatures = await unitOfWork.Repository<Feature>().CountAsync(f => true);
        var totalInsurance = await unitOfWork.Repository<Insurance>().CountAsync(i => true);

        // 2. Unit Status Distribution (Efficient)
        var forSaleCount = await unitOfWork.Repository<Unit>().CountAsync(u => u.Status == UnitStatus.Sale);
        var forRentCount = await unitOfWork.Repository<Unit>().CountAsync(u => u.Status == UnitStatus.Rent);
        var soldCount = await unitOfWork.Repository<Unit>().CountAsync(u => u.Status == UnitStatus.Sold);
        var rentedCount = await unitOfWork.Repository<Unit>().CountAsync(u => u.Status == UnitStatus.Rented);


        // 3. Project Status Distribution
        var projectSaleCount = await unitOfWork.Repository<Project>().CountAsync(p => p.Status == ProjectStatus.Sale);
        var projectRentCount = await unitOfWork.Repository<Project>().CountAsync(p => p.Status == ProjectStatus.Rent);
        var projectSoldCount = await unitOfWork.Repository<Project>().CountAsync(p => p.Status == ProjectStatus.Sold);
        var projectRentedCount = await unitOfWork.Repository<Project>().CountAsync(p => p.Status == ProjectStatus.Rented);


        // 4. Data for Insights (Fetch only what's needed for complex logic)
        var projectsData = await unitOfWork.Repository<Project>().GetAllAsync(asNoTracking: true);
        var activeProjects = projectsData.ToList();

        var unitsForInsights = await unitOfWork.Repository<Unit>().GetAllAsync(asNoTracking: true);
        var unitsList = unitsForInsights.ToList();

        var typeDistribution = unitsList
            .GroupBy(u => u.Type)
            .Select(g => new { type = g.Key.ToString(), count = g.Count() })
            .ToList();

        var totalInventoryValue = unitsList
            .Where(u => u.Status == UnitStatus.Sale && u.Price.HasValue)
            .Sum(u => u.Price!.Value);


        var avgPricePerUnit = unitsList
            .Where(u => u.Price.HasValue)
            .Select(u => u.Price!.Value)
            .DefaultIfEmpty(0)
            .Average();

        var topPerformingProjects = activeProjects
            .Select(p => {
                var total = p.TransactedUnitsCount + p.AvailableUnitsCount;
                var sold = p.TransactedUnitsCount;
                return new {
                    projectId = p.ProjectId,
                    name = p.Name,
                    salesProgress = total > 0 ? Math.Round((double)sold / total * 100, 2) : 0,
                    totalUnits = total
                };
            })
            .OrderByDescending(x => x.salesProgress)
            .Take(5)
            .ToList();

        var cityInsights = activeProjects
            .GroupBy(p => p.City ?? "Other")
            .Select(g => new { 
                city = g.Key, 
                projects = g.Count(),
                totalUnits = g.Sum(p => p.TransactedUnitsCount + p.AvailableUnitsCount)
            })
            .OrderByDescending(x => x.projects)
            .ToList();

        return new
        {
            summary = new
            {
                totalProjects,
                totalBuildings,
                totalUnits,
                totalBlogs,
                totalMessages,
                totalFeatures,
                totalInsurance
            },
            inventoryStatus = new
            {
                forSale = forSaleCount,
                forRent = forRentCount,
                sold = soldCount,
                rented = rentedCount,
                occupancyRate = totalUnits > 0 ? Math.Round((double)(soldCount + rentedCount) / totalUnits * 100, 2) : 0
            },
            projectHealth = new
            {
                sale = projectSaleCount,
                rent = projectRentCount,
                sold = projectSoldCount,
                rented = projectRentedCount
            },

            insights = new
            {
                typeDistribution,
                cityInsights,
                topPerformingProjects,
                financials = new
                {
                    totalInventoryValue,
                    avgPrice = Math.Round(avgPricePerUnit, 2)
                }
            }
        };
    }

    public async Task<object> GetChartAsync()
    {
        return new
        {
            labels = new[] { "يناير", "فبراير", "مارس", "أبريل" },
            datasets = new[] { new { label = "الطلبات الجديدة", data = new[] { 15, 20, 10, 25 } } }
        };
    }
}
