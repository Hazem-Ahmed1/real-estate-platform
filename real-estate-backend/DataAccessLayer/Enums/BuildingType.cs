namespace DataAccessLayer.Enums;

/// <summary>
/// A building can ONLY contain Sale/Sold units OR Rent/Rented units — never mixed.
/// </summary>
public enum BuildingType
{
    Sale = 1,
    Rent = 2
}
