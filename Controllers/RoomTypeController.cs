using Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyService.DTOs;
using PropertyService.Services;

namespace PropertyService.Controllers;

[ApiController]
[Route("room-types")]
[Authorize(Roles = "admin")]
public class RoomTypeController(RoomTypeService roomTypeService) : ControllerBase
{
    private readonly RoomTypeService _roomTypeService = roomTypeService;

    [HttpPost]
    public async Task<IActionResult> Create(CreateRoomTypeDto dto)
    {
        long id = await _roomTypeService.CreateAsync(dto);

        var response = HttpApiResponseDto<object>.Success(
            new { id = id.ToString() },
            "Room type created successfully"
        );

        return StatusCode(StatusCodes.Status201Created, response);
    }
}