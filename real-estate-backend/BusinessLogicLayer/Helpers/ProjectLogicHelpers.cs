namespace BusinessLogicLayer.Helpers;

public static class ProjectLogicHelpers
{
    /// <summary>
    /// Derives a single Status for a building given its Type and units.
    ///   Sale building → Sale (if any Sale) or Sold (if ALL are Sold)
    ///   Rent building → Rent (if any Rent) or Rented (if ALL are Rented)
    /// Returns null when the building has no units.
    /// </summary>
    public static UnitStatus? DeriveBuildingStatus(BuildingType type, IEnumerable<Unit> units)
    {
        var list = units.ToList();
        if (!list.Any()) return null;

        if (type == BuildingType.Sale)
            return list.All(u => u.Status == UnitStatus.Sold) ? UnitStatus.Sold : UnitStatus.Sale;

        // Rent building
        return list.All(u => u.Status == UnitStatus.Rented) ? UnitStatus.Rented : UnitStatus.Rent;
    }

    /// <summary>
    /// Derives a single ProjectStatus from all units across all buildings.
    ///
    /// Rules:
    ///   - Only Sale/Sold units  → Sale, or Sold when ALL are Sold.
    ///   - Only Rent/Rented units → Rent, or Rented when ALL are Rented.
    ///   - Mixed project (both types exist):
    ///       * If the project was on the Sale path keep Sale unless ALL sale AND rent units
    ///         are transacted → stays Sale (can only reach Sold when ALL sale units are Sold).
    ///       * If on Rent path — symmetric.
    ///     In short: the primary status follows `currentStatus` path; the other group is ignored
    ///     for the primary status but its building's own Status still tracks correctly.
    /// </summary>
    public static ProjectStatus DeriveProjectStatus(IEnumerable<Unit> units, ProjectStatus currentStatus)
    {
        var list = units.ToList();
        if (!list.Any()) return currentStatus; // no units → keep whatever was set

        var saleUnits = list.Where(u => u.Status == UnitStatus.Sale || u.Status == UnitStatus.Sold).ToList();
        var rentUnits = list.Where(u => u.Status == UnitStatus.Rent || u.Status == UnitStatus.Rented).ToList();

        // Pure sale project
        if (saleUnits.Any() && !rentUnits.Any())
            return saleUnits.All(u => u.Status == UnitStatus.Sold) ? ProjectStatus.Sold : ProjectStatus.Sale;

        // Pure rent project
        if (rentUnits.Any() && !saleUnits.Any())
            return rentUnits.All(u => u.Status == UnitStatus.Rented) ? ProjectStatus.Rented : ProjectStatus.Rent;

        // Mixed project — follow the current primary path
        bool isSalePath = currentStatus == ProjectStatus.Sale || currentStatus == ProjectStatus.Sold;
        if (isSalePath)
            return saleUnits.All(u => u.Status == UnitStatus.Sold) ? ProjectStatus.Sold : ProjectStatus.Sale;
        else
            return rentUnits.All(u => u.Status == UnitStatus.Rented) ? ProjectStatus.Rented : ProjectStatus.Rent;
    }

    public static ProjectStatus MapUnitStatusToProjectStatus(UnitStatus unitStatus)
    {
        return unitStatus switch
        {
            UnitStatus.Sale   => ProjectStatus.Sale,
            UnitStatus.Sold   => ProjectStatus.Sold,
            UnitStatus.Rent   => ProjectStatus.Rent,
            UnitStatus.Rented => ProjectStatus.Rented,
            _                 => ProjectStatus.Sale
        };
    }
}
