using System.ComponentModel.DataAnnotations;

namespace PropertyService.DTOs;

public class CreatePropertyDto
{
    [Required(ErrorMessage = "Property name is required.")]
    [MaxLength(150, ErrorMessage = "Property name must be at most 150 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required.")]
    [MaxLength(150, ErrorMessage = "Address must be at most 150 characters.")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ward is required.")]
    [MaxLength(100, ErrorMessage = "Ward must be at most 100 characters.")]
    public string Ward { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required.")]
    [MaxLength(50, ErrorMessage = "City must be at most 50 characters.")]
    public string City { get; set; } = string.Empty;

    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
    public double Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
    public double Longitude { get; set; }

    public TimeOnly CheckInTime { get; set; }
    public TimeOnly CheckOutTime { get; set; }
}
