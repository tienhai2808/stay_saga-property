namespace PropertyService.DTOs;

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