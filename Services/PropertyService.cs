using Common.Exceptions;
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
}
