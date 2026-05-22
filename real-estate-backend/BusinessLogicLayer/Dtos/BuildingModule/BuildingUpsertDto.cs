using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.Dtos.BuildingModule;

public class BuildingUpsertDto
{
    [Required(ErrorMessage = "Building name is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Building name must be between 2 and 200 characters.")]
    public string Name { get; set; } = string.Empty;



    [Required(ErrorMessage = "ProjectId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "ProjectId must be a positive number (greater than 0).")]
    public int ProjectId { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public double MaxArea { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public double BuildingArea { get; set; }

    [Required(ErrorMessage = "Building type is required (Sale, Rent, or Both).")]
    public BuildingType Type { get; set; }
    [Range(0, 30)]
    public int? FloorCount { get; set; }
}

