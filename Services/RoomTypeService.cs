using Common.DTOs;
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

    public async Task<long> CreateAsync(CreateRoomTypeDto dto, CancellationToken cancellationToken = default)
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
            MaxGuest = dto.MaxGuest,
            TotalRoom = dto.TotalRoom
        };

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var endExclusiveDate = startDate.AddMonths(18);

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _roomTypeRepo.CreateAsync(roomType, cancellationToken);
            await _roomInventoryRepo.CreateBulkAsync(
                roomType.Id,
                roomType.TotalRoom,
                startDate,
                endExclusiveDate,
                cancellationToken
            );

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return roomType.Id;
    }

    public async Task<(List<RoomTypeResponseDto>, MetaResponseDto)> ListAsync(
        RoomTypeQueryDto dto, 
        CancellationToken cancellationToken = default
    )
    {
        long? propertyId = null;
        if (!string.IsNullOrWhiteSpace(dto.PropertyId))
        {
            if (!long.TryParse(dto.PropertyId.Trim(), out var parsedPropertyId) || parsedPropertyId <= 0)
            {
                throw new ValidationException("Invalid property id");
            }

            propertyId = parsedPropertyId;
        }

        var sort = string.IsNullOrWhiteSpace(dto.Sort)
            ? "id"
            : dto.Sort.Trim().ToLowerInvariant();

        var order = dto.Order.Trim();
        var isDescending = false;
        if (!string.IsNullOrWhiteSpace(order))
        {
            isDescending = order.Equals("desc", StringComparison.OrdinalIgnoreCase);
            var isAscending = order.Equals("asc", StringComparison.OrdinalIgnoreCase);
            if (!isDescending && !isAscending)
            {
                throw new ValidationException("Order must be either 'asc' or 'desc'");
            }
        }

        var validSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "id",
            "name",
            "price"
        };
        if (!validSortFields.Contains(sort))
        {
            throw new ValidationException("Sort must be one of: id, name, price");
        }

        var (roomTypes, total) = await _roomTypeRepo.ListAsync(
            propertyId,
            dto.Search,
            sort,
            isDescending,
            dto.Page,
            dto.Limit,
            cancellationToken
        );

        var roomTypesRes = roomTypes
            .Select(rt => new RoomTypeResponseDto(
                rt.Id.ToString(),
                rt.Name,
                rt.Price,
                rt.MaxGuest,
                rt.TotalRoom,
                new BasicPropertyResponseDto(
                    rt.Property.Id.ToString(),
                    rt.Property.Name,
                    rt.Property.Address,
                    rt.Property.Ward,
                    rt.Property.City
                )
            ))
            .ToList();
        
        var totalPage = total == 0 ? 0 : (int)Math.Ceiling(total / (double)dto.Limit);

        var meta = new MetaResponseDto(
            total,
            dto.Page,
            dto.Limit,
            totalPage,
            dto.Page > 1 && totalPage > 0,
            dto.Page < totalPage
        );

        return (roomTypesRes, meta);
    }

    public async Task UpdateAsync(
        long id, 
        UpdateRoomTypeDto dto, 
        CancellationToken cancellationToken = default
    )
    {
        var roomType = await _roomTypeRepo.GetById(id, cancellationToken) ??
            throw new NotFoundException("Room type not found");

        if (!long.TryParse(dto.PropertyId.Trim(), out long propertyId) || propertyId <= 0)
        {
            throw new ValidationException("Invalid property id");
        }

        roomType.Name = dto.Name.Trim();
        roomType.MaxGuest = dto.MaxGuest;
        roomType.PropertyId = propertyId;
        roomType.Price = dto.Price;
        roomType.TotalRoom = dto.TotalRoom;

        await _roomTypeRepo.UpdateAsync(roomType);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _roomTypeRepo.DeleteAsync(id, cancellationToken);
    }
}
