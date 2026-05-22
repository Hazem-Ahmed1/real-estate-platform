namespace BusinessLogicLayer.Contracts;

public interface IDashboardService
{
    Task<object> GetStatsAsync();
    Task<object> GetChartAsync();
    Task<object> GetPublicStatsAsync();
}
