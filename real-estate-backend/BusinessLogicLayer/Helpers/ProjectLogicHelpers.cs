
namespace BusinessLogicLayer.Helpers;

public static class ProjectLogicHelpers
{
    public static ProjectStatus DeriveProjectStatus(IEnumerable<Unit> units, ProjectStatus currentStatus)
    {
        var unitList = units.ToList();
        bool isSaleType = currentStatus == ProjectStatus.Sale || currentStatus == ProjectStatus.Sold;

        if (unitList.Count == 0)
        {
            return isSaleType ? ProjectStatus.Sale : ProjectStatus.Rent;
        }

        if (isSaleType)
        {
            return unitList.All(u => u.Status == UnitStatus.Sold) ? ProjectStatus.Sold : ProjectStatus.Sale;
        }
        else
        {
            return unitList.All(u => u.Status == UnitStatus.Rented) ? ProjectStatus.Rented : ProjectStatus.Rent;
        }
    }


}
