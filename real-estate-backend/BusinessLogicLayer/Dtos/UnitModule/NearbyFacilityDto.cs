using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.Dtos.UnitModule;

public class NearbyFacilityDto
{
    [Required(ErrorMessage = "Facility name is required.")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Facility name must be between 1 and 255 characters.")]
    public string Name { get; set; } = null!;
    
    [Required(ErrorMessage = "Facility type is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Facility type must be between 1 and 100 characters.")]
    public string Type { get; set; } = null!;
    
    [Required(ErrorMessage = "Distance is required.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Distance must be between 1 and 50 characters.")]
    public string Distance { get; set; } = null!;
    
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double Area { get; set; }
}
