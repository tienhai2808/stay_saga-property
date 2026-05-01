using Common.DTOs;
using Common.Exceptions;
using IdGen;
using PropertyService.DTOs;
using PropertyService.Models;
using PropertyService.Repositories;

namespace PropertyService.Services;

public class PropertyService(PropertyRepository propertyRepo, RoomTypeRepository roomTypeRepo, IIdGenerator<long> idGenerator)
{
    private readonly PropertyRepository _propertyRepo = propertyRepo;
    private readonly RoomTypeRepository _roomTypeRepo = roomTypeRepo;
    private readonly IIdGenerator<long> _idGenerator = idGenerator;

    public async Task<long> CreateAsync(CreatePropertyDto dto)
    {
        var property = new Property
        {
            Id = _idGenerator.CreateId(),
            Name = dto.Name.Trim(),
            Address = dto.Address.Trim(),
            Ward = dto.Ward.Trim(),
            City = dto.City.Trim(),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            CheckInTime = dto.CheckInTime,
            CheckOutTime = dto.CheckOutTime
        };

        await _propertyRepo.CreateAsync(property);

        return property.Id;
    }

    public async Task UpdateAsync(long id, UpdatePropertyDto dto)
    {
        var property = await _propertyRepo.GetByIdAsync(id) ?? 
            throw new NotFoundException("Property not found");
        
        property.Name = dto.Name.Trim();
        property.Address = dto.Address.Trim();
        property.Ward = dto.Ward.Trim();
        property.City = dto.City.Trim();
        property.Latitude = dto.Latitude;
        property.Longitude = dto.Longitude;
        property.CheckInTime = dto.CheckInTime;
        property.CheckOutTime = dto.CheckOutTime;

        await _propertyRepo.UpdateAsync(property);
    }

    public async Task DeleteAsync(long id)
    {
        await _propertyRepo.DeleteAsync(id);
    }

    public async Task<(List<PropertyResponseDto>, MetaResponseDto)> ListAsync(PropertyQueryDto dto)
    {
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
            "address",
            "ward",
            "city"
        };
        if (!validSortFields.Contains(sort))
            throw new ValidationException("Sort must be one of: id, name, address, ward, city");

        var (properties, total) = await _propertyRepo.ListAsync(
            dto.Search,
            sort,
            isDescending,
            dto.Page,
            dto.Limit
        );

        var propertyRes = properties
            .Select(p => new PropertyResponseDto(
                p.Id.ToString(),
                p.Name,
                p.Address,
                p.Ward,
                p.City,
                p.Latitude,
                p.Longitude,
                p.CheckInTime,
                p.CheckOutTime
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

        return (propertyRes, meta);
    }

    public async Task<(PropertyResponseDto?, List<BasicRoomTypeResponseDto>, MetaResponseDto)> ListRoomTypesByIdAsync(long id, RoomTypeQueryDto dto, bool includeProperty)
    {
        PropertyResponseDto? propertyRes = null;

        if (includeProperty)
        {
            var property = await _propertyRepo.GetByIdAsync(id) ??
                throw new NotFoundException("Property not found");
            
            propertyRes = new PropertyResponseDto(
                property.Id.ToString(),
                property.Name,
                property.Address,
                property.Ward,
                property.City,
                property.Latitude,
                property.Longitude,
                property.CheckInTime,
                property.CheckOutTime
            );
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
            "price",
        };

        if (!validSortFields.Contains(sort))
            throw new ValidationException("Sort must be one of: id, name, price");

        var (roomTypes, total) = await _roomTypeRepo.ListByPropertyIdAsync(
            id,
            dto.Search,
            sort,
            isDescending,
            dto.Page,
            dto.Limit
        );

        var roomTypesRes = roomTypes
            .Select(rt => new BasicRoomTypeResponseDto(
                rt.Id.ToString(),
                rt.Name,
                rt.Price,
                rt.MaxGuest,
                rt.TotalRoom
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

        return (propertyRes, roomTypesRes, meta);
    }
}
