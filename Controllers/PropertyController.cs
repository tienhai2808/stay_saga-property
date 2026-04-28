using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyService.DTOs;
using PropertyDomainService = PropertyService.Services.PropertyService;
using Common.DTOs;

namespace PropertyService.Controllers;

[ApiController]
[Route("properties")]
public class PropertyController(PropertyDomainService propertyService) : ControllerBase
{
    private readonly PropertyDomainService _propertyService = propertyService;

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create(CreatePropertyDto dto)
    {
        long id = await _propertyService.CreateAsync(dto);

        var response = HttpApiResponseDto<object>.Success(
            new { id = id.ToString() },
            "Property created successful"
        );

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(long id, UpdatePropertyDto dto)
    {
        await _propertyService.UpdateAsync(id, dto);

        var response = HttpApiResponseDto<object>.Success(
            null,
            "Property updated successfully"
        );

        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(long id)
    {
        await _propertyService.DeleteAsync(id);

        var response = HttpApiResponseDto<object>.Success(
            null,
            "Property deleted successfully"
        );

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] PropertyQueryDto dto)
    {
        var (propertiesRes, meta) = await _propertyService.ListAsync(dto);

        var response =HttpApiResponseDto<object>.Success(
            new
            {
                properties = propertiesRes,
                meta,
            }
        );

        return Ok(response);
    }
}
