using System.Globalization;
using Grpc.Core;
using Grpc.Property;
using PropertyService.Data;
using PropertyService.Models;
using PropertyService.Repositories;
using PropertyGrpc = Grpc.Property.PropertyService;

namespace PropertyService.Services;

public sealed class GrpcService(
    RoomTypeRepository roomTypeRepo,
    RoomInventoryRepository roomInventoryRepo,
    AppDbContext db
) : PropertyGrpc.PropertyServiceBase
{
    private readonly RoomTypeRepository _roomTypeRepo = roomTypeRepo;
    private readonly RoomInventoryRepository _roomInventoryRepo = roomInventoryRepo;
    private readonly AppDbContext _db = db;

    public override Task<EmptyResponse> Ping(PingRequest request, ServerCallContext context)
    {
        return Task.FromResult(new EmptyResponse());
    }

    public override async Task<ReserveResponse> Reserve(ReserveRequest request, ServerCallContext context)
    {
        var cancellationToken = context.CancellationToken;
        if (
            !DateOnly.TryParseExact(
                request.CheckIn,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var checkIn
            )
        )
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Invalid check-in date format. Expected yyyy-MM-dd")
            );

        if (
            !DateOnly.TryParseExact(
                request.CheckOut,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var checkOut
            )
        )
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Invalid check-out date format. Expected yyyy-MM-dd")
            );

        var roomType = await _roomTypeRepo.FindByIdAsync(request.RoomTypeId, cancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Room type not found"));

        if (request.RoomCount > roomType.TotalRoom)
            throw new RpcException(
                new Status(StatusCode.FailedPrecondition, "Requested room count exceeds room type capacity")
            );

        var maxAllowedGuests = roomType.MaxGuest * request.RoomCount;
        if (request.GuestCount > maxAllowedGuests)
            throw new RpcException(
                new Status(StatusCode.FailedPrecondition, $"Guest count exceeds capacity for {request.RoomCount} room(s)")
            );

        var totalNights = checkOut.DayNumber - checkIn.DayNumber;

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var inventoryRows = await _roomInventoryRepo.CountByRoomTypeIdAndStatusesInDateRangeAsync(
            roomType.Id,
            checkIn,
            checkOut,
            cancellationToken: cancellationToken
        );
        if (inventoryRows != totalNights)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new RpcException(
                new Status(StatusCode.FailedPrecondition, "Room inventory is not available for selected dates")
            );
        }

        var reservableRows = await _roomInventoryRepo.CountByRoomTypeIdAndStatusesInDateRangeAsync(
            roomType.Id,
            checkIn,
            checkOut,
            minAvailableCount: request.RoomCount,
            statuses: [RoomInventoryStatuses.Available],
            cancellationToken: cancellationToken
        );
        if (reservableRows != totalNights)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new RpcException(
                new Status(StatusCode.FailedPrecondition, "Not enough rooms available for selected dates")
            );
        }

        var reservedRows = await _roomInventoryRepo.UpdateAvailableCountByRoomTypeIdInDateRangeAsync(
            roomType.Id,
            -request.RoomCount,
            checkIn,
            checkOut,
            cancellationToken
        );
        if (reservedRows != totalNights)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new RpcException(
                new Status(StatusCode.FailedPrecondition, "Not enough rooms available for selected dates")
            );
        }

        await _roomInventoryRepo.UpdateStatusByRoomTypeIdInDateRangeAsync(
            roomType.Id,
            checkIn,
            checkOut,
            RoomInventoryStatuses.Booked,
            minAvailableCount: 0,
            maxAvailableCount: 0,
            cancellationToken: cancellationToken
        );

        await transaction.CommitAsync(cancellationToken);

        var amount = roomType.Price * request.RoomCount * totalNights;

        return new ReserveResponse { Amount = (float)amount };
    }

    public override async Task<EmptyResponse> Release(ReleaseRequest request, ServerCallContext context)
    {
        var cancellationToken = context.CancellationToken;
        if (
            !DateOnly.TryParseExact(
                request.CheckIn,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var checkIn
            )
        )
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Invalid check-in date format. Expected yyyy-MM-dd")
            );

        if (
            !DateOnly.TryParseExact(
                request.CheckOut,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var checkOut
            )
        )
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Invalid check-out date format. Expected yyyy-MM-dd")
            );

        var roomType = await _roomTypeRepo.FindByIdAsync(request.RoomTypeId, cancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Room type not found"));

        if (request.RoomCount > roomType.TotalRoom)
            throw new RpcException(
                new Status(StatusCode.FailedPrecondition, "Requested room count exceeds room type capacity")
            );

        var totalNights = checkOut.DayNumber - checkIn.DayNumber;

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var inventoryRows = await _roomInventoryRepo.CountByRoomTypeIdAndStatusesInDateRangeAsync(
            roomType.Id,
            checkIn,
            checkOut,
            cancellationToken: cancellationToken
        );
        if (inventoryRows != totalNights)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new RpcException(
                new Status(StatusCode.FailedPrecondition, "Room inventory is not available for selected dates")
            );
        }

        var releasableRows = await _roomInventoryRepo.CountByRoomTypeIdAndStatusesInDateRangeAsync(
            roomType.Id,
            checkIn,
            checkOut,
            maxAvailableCount: roomType.TotalRoom - request.RoomCount,
            statuses: [RoomInventoryStatuses.Available, RoomInventoryStatuses.Booked],
            cancellationToken: cancellationToken
        );
        if (releasableRows != totalNights)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new RpcException(
                new Status(StatusCode.FailedPrecondition, "Release room count exceeds reserved inventory")
            );
        }

        var releasedRows = await _roomInventoryRepo.UpdateAvailableCountByRoomTypeIdInDateRangeAsync(
            roomType.Id,
            request.RoomCount,
            checkIn,
            checkOut,
            cancellationToken
        );
        if (releasedRows != totalNights)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new RpcException(
                new Status(StatusCode.FailedPrecondition, "Release room count exceeds reserved inventory")
            );
        }

        await _roomInventoryRepo.UpdateStatusByRoomTypeIdInDateRangeAsync(
            roomType.Id,
            checkIn,
            checkOut,
            RoomInventoryStatuses.Available,
            minAvailableCount: 1,
            statuses: [RoomInventoryStatuses.Booked],
            cancellationToken: cancellationToken
        );

        await transaction.CommitAsync(cancellationToken);

        return new EmptyResponse();
    }
}
