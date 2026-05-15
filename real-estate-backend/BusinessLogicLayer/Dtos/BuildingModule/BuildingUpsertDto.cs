using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.Dtos.BuildingModule;

public class BuildingUpsertDto
{
    [Required(ErrorMessage = "Building name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Building name must be between 1 and 200 characters.")]
    public string Name { get; set; } = string.Empty;



    [Required(ErrorMessage = "ProjectId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "ProjectId must be a positive number (greater than 0).")]
    public int ProjectId { get; set; }

    [Range(0, double.MaxValue)]
    public double? MaxArea { get; set; }

    [Range(0, double.MaxValue)]
    public double? BuildingArea { get; set; }

    [Range(1, 1000)]
    public int? FloorCount { get; set; }
}

