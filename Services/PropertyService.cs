using IdGen;
using PropertyService.DTOs;
using PropertyService.Models;
using PropertyService.Repositories;

namespace PropertyService.Services;

public class PropertyService(PropertyRepository propertyRepo, IIdGenerator<long> idGenerator)
{
    private readonly PropertyRepository _propertyRepo = propertyRepo;
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
}
