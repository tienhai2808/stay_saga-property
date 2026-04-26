using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyService.DTOs;
using PropertyDomainService = PropertyService.Services.PropertyService;
using Common.Response;

namespace PropertyService.Controllers;

[ApiController]
[Route("properties")]
[Authorize(Roles = "admin")]
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, UpdatePropertyDto dto)
    {
        await _propertyService.UpdateAsync(id, dto);

        var response = HttpApiResponse<object>.Success(
            null,
            "Property updated successfully"
        );

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _propertyService.DeleteAsync(id);

        var response = HttpApiResponse<object>.Success(
            null,
            "Property deleted successfully"
        );

        return Ok(response);
    }
}
