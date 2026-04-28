using Common.DTOs;

namespace PropertyService.DTOs;

public class PropertyQueryDto: PaginationQueryDto
{
    public string Search { get; set; } = string.Empty;
}