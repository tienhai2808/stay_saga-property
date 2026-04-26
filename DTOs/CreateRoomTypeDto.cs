using System.ComponentModel.DataAnnotations;

namespace PropertyService.DTOs;

public class CreateRoomTypeDto
{
    [Required(ErrorMessage = "Room type name is required.")]
    [MaxLength(100, ErrorMessage = "Room type name must be at most 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Property id is required.")]
    [RegularExpression("^[0-9]+$", ErrorMessage = "Property id must contain digits only.")]
    [MaxLength(20, ErrorMessage = "Property id must be at most 20 characters.")]
    public string PropertyId { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Max people must be greater than 0.")]
    public int MaxPeople { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Total room must be greater than 0.")]
    public int TotalRoom { get; set; }
}
