using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyService.DTOs;
using PropertyDomainService = PropertyService.Services.PropertyService;
using Common.DTOs;

namespace PropertyService.Controllers;

[ApiController]
[Route("properties")]
[Authorize]
public class PropertyController(PropertyDomainService propertyService) : ControllerBase
{
    private readonly PropertyDomainService _propertyService = propertyService;

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create(CreatePropertyDto dto, CancellationToken cancellationToken)
    {
        long id = await _propertyService.CreateAsync(dto, cancellationToken);

        var response = HttpApiResponseDto<object>.Success(
            new { id = id.ToString() },
            "Property created successful"
        );

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(long id, UpdatePropertyDto dto, CancellationToken cancellationToken)
    {
        await _propertyService.UpdateAsync(id, dto, cancellationToken);

        var response = HttpApiResponseDto<object>.Success(
            null,
            "Property updated successfully"
        );

        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await _propertyService.DeleteAsync(id, cancellationToken);

        var response = HttpApiResponseDto<object>.Success(
            null,
            "Property deleted successfully"
        );

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] PropertyQueryDto dto, CancellationToken cancellationToken)
    {
        var (propertiesRes, meta) = await _propertyService.ListAsync(dto, cancellationToken);

        var response = HttpApiResponseDto<object>.Success(
            new
            {
                properties = propertiesRes,
                meta,
            }
        );

        return Ok(response);
    }

    [HttpGet("{id}/room-types")]
    public async Task<IActionResult> ListRoomTypesByID(long id, [FromQuery] RoomTypeQueryDto dto, CancellationToken cancellationToken)
    {
        var includeProperty = dto.Include == "property";

        var (propertyRes, roomTypesRes, meta) = await _propertyService.ListRoomTypesByIdAsync(id, dto, includeProperty, cancellationToken);

        var resData = new Dictionary<string, object>
        {
            ["roomTypes"] = roomTypesRes,
            ["meta"] = meta
        };

        if (includeProperty && propertyRes != null)
            resData["property"] = propertyRes;
        
        var response = HttpApiResponseDto<object>.Success(
            resData
        );

        return Ok(response);
    }
}
