namespace PropertyService.DTOs;

public sealed record BasicPropertyResponseDto(
    string Id,
    string Name,
    string Address,
    string Ward,
    string City
);

public sealed record PropertyResponseDto(
    string Id,
    string Name,
    string Address,
    string Ward,
    string City,
    double Latitude,
    double Longitude,
    TimeOnly CheckInTime,
    TimeOnly CheckOutTime
);