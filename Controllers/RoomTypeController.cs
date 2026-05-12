using Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyService.DTOs;
using PropertyService.Services;

namespace PropertyService.Controllers;

[ApiController]
[Route("room-types")]
[Authorize]
public class RoomTypeController(RoomTypeService roomTypeService) : ControllerBase
{
    private readonly RoomTypeService _roomTypeService = roomTypeService;

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create(CreateRoomTypeDto dto, CancellationToken cancellationToken)
    {
        long id = await _roomTypeService.CreateAsync(dto, cancellationToken);

        var response = HttpApiResponseDto<object>.Success(
            new { id = id.ToString() },
            "Room type created successfully"
        );

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] RoomTypeQueryDto dto, CancellationToken cancellationToken)
    {
        var (roomTypesRes, meta) = await _roomTypeService.ListAsync(dto, cancellationToken);

        var response = HttpApiResponseDto<object>.Success(
            new
            {
                roomTypes = roomTypesRes,
                meta
            }
        );

        return Ok(response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(long id, UpdateRoomTypeDto dto, CancellationToken cancellationToken)
    {
        await _roomTypeService.UpdateAsync(id, dto, cancellationToken);

        var response = HttpApiResponseDto<object>.Success(
            null,
            "Room type updated successfully"
        );

        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await _roomTypeService.DeleteAsync(id, cancellationToken);

        var response = HttpApiResponseDto<object>.Success(
            null,
            "Room type deleted successfully"
        );

        return Ok(response);
    }
}