using System.ComponentModel.DataAnnotations;
using Common.DTOs;

namespace PropertyService.DTOs;

public class RoomTypeQueryDto: PaginationQueryDto
{
    
    public string Search { get; set; } = string.Empty;

    [RegularExpression("^[0-9]+$", ErrorMessage = "Property id must contain digits only.")]
    [MaxLength(20, ErrorMessage = "Property id must be at most 20 characters.")]
    public string PropertyId { get; set; } = string.Empty;

    [RegularExpression("^property$", ErrorMessage = "Include must be 'property'.")]
    public string Include { get; set; } = string.Empty;
}