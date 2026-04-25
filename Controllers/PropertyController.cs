using Microsoft.AspNetCore.Mvc;
using PropertyService.DTOs;
using PropertyDomainService = PropertyService.Services.PropertyService;
using Common.Response;

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

        var response = HttpApiResponse<object>.Success(
            new { id = id.ToString() },
            "Property created successful"
        );

        return StatusCode(StatusCodes.Status201Created, response);
    }
}
