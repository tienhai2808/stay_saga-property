using Microsoft.AspNetCore.Mvc;
using PropertyService.DTOs;
using PropertyDomainService = PropertyService.Services.PropertyService;

namespace PropertyService.Controllers;

[ApiController]
[Route("properties")]
public class PropertyController(PropertyDomainService propertyService) : ControllerBase
{
    private readonly PropertyDomainService _propertyService = propertyService;

    [HttpPost]
    public async Task<IActionResult> Create(CreatePropertyDto dto)
    {
        long id = await _propertyService.CreateAsync(dto);

        var response = new ApiResponse<object>(
          StatusCodes.Status201Created,
          "Property created successful",
          new { id = id.ToString() }
        );

        return StatusCode(StatusCodes.Status201Created, response);
    }
}
