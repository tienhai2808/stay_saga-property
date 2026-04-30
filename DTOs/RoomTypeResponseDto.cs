namespace PropertyService.DTOs;

public sealed record BasicRoomTypeResponseDto(
    string Id,
    string Name,
    decimal Price,
    int MaxGuest,
    int TotalRoom
);

public sealed record RoomTypeResponseDto (
    string Id,
    string Name,
    decimal Price,
    int MaxGuest,
    int TotalRoom,
    BasicPropertyResponseDto Property
);