using Common.Exceptions;
using IdGen;
using PropertyService.Data;
using PropertyService.DTOs;
using PropertyService.Models;
using PropertyService.Repositories;

namespace PropertyService.Services;

public class RoomTypeService(
    RoomTypeRepository roomTypeRepo,
    RoomInventoryRepository roomInventoryRepo,
    IIdGenerator<long> idGenerator,
    AppDbContext db
)
{
    private readonly RoomTypeRepository _roomTypeRepo = roomTypeRepo;
    private readonly RoomInventoryRepository _roomInventoryRepo = roomInventoryRepo;
    private readonly IIdGenerator<long> _idGenerator = idGenerator;
    private readonly AppDbContext _db = db;

    public async Task<long> CreateAsync(CreateRoomTypeDto dto)
    {
        if (!long.TryParse(dto.PropertyId.Trim(), out long propertyId) || propertyId <= 0)
        {
            throw new ValidationException("Invalid property id");
        }

        var roomType = new RoomType
        {
            Id = _idGenerator.CreateId(),
            Name = dto.Name.Trim(),
            PropertyId = propertyId,
            Price = dto.Price,
            MaxGuest = dto.MaxPeople,
            TotalRoom = dto.TotalRoom
        };

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var endExclusiveDate = startDate.AddMonths(18);

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _roomTypeRepo.CreateAsync(roomType);
            await _roomInventoryRepo.CreateBulkAsync(
                roomType.Id,
                roomType.TotalRoom,
                startDate,
                endExclusiveDate
            );

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return roomType.Id;
    }
}
