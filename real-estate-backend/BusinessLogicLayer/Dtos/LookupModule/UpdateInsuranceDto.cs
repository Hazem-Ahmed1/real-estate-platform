using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer.Dtos.LookupModule;

public class UpdateInsuranceDto
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "Duration must be between 1 and 100 years.")]
    public int Duration { get; set; }

    public bool IsActive { get; set; }
}
