using Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PropertyService.Data;
using PropertyService.Models;

namespace PropertyService.Repositories;

public class RoomInventoryRepository(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task CreateBulkAsync(
        long roomTypeId,
        int totalRoom,
        DateOnly startDate,
        DateOnly endExclusiveDate,
        CancellationToken cancellationToken = default
    )
    {
        var inventories = new List<RoomInventory>();

        for (var date = startDate; date < endExclusiveDate; date = date.AddDays(1))
        {
            inventories.Add(new RoomInventory
            {
                RoomTypeId = roomTypeId,
                Date = date,
                AvailableCount = totalRoom,
                Status = RoomInventoryStatuses.Available
            });
        }

        _db.RoomInventories.AddRange(inventories);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException pg &&
            pg.SqlState == PostgresErrorCodes.UniqueViolation
        )
        {
            throw new ConflictException("Room inventory already exists for one or more dates");
        }
    }

    public Task<int> CountByRoomTypeIdAndStatusesInDateRangeAsync(
        long roomTypeId,
        DateOnly checkIn,
        DateOnly checkOut,
        int? minAvailableCount = null,
        int? maxAvailableCount = null,
        string[]? statuses = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _db.RoomInventories.Where(ri =>
            ri.RoomTypeId == roomTypeId && ri.Date >= checkIn && ri.Date < checkOut
        );

        if (minAvailableCount.HasValue)
            query = query.Where(ri => ri.AvailableCount >= minAvailableCount.Value);

        if (maxAvailableCount.HasValue)
            query = query.Where(ri => ri.AvailableCount <= maxAvailableCount.Value);

        if (statuses is { Length: > 0 })
            query = query.Where(ri => statuses.Contains(ri.Status));

        return query.CountAsync(cancellationToken);
    }

    public Task<int> UpdateAvailableCountByRoomTypeIdInDateRangeAsync(
        long roomTypeId,
        int availableCountDelta,
        DateOnly checkIn,
        DateOnly checkOut,
        CancellationToken cancellationToken = default
    ) => _db.RoomInventories
            .Where(ri =>
                ri.RoomTypeId == roomTypeId
                && ri.Date >= checkIn
                && ri.Date < checkOut
            )
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(ri => ri.AvailableCount, ri => ri.AvailableCount + availableCountDelta),
                cancellationToken
            );

    public Task<int> UpdateStatusByRoomTypeIdInDateRangeAsync(
        long roomTypeId,
        DateOnly checkIn,
        DateOnly checkOut,
        string status,
        int? minAvailableCount = null,
        int? maxAvailableCount = null,
        string[]? statuses = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _db.RoomInventories.Where(ri =>
            ri.RoomTypeId == roomTypeId && ri.Date >= checkIn && ri.Date < checkOut
        );

        if (minAvailableCount.HasValue)
            query = query.Where(ri => ri.AvailableCount >= minAvailableCount.Value);

        if (maxAvailableCount.HasValue)
            query = query.Where(ri => ri.AvailableCount <= maxAvailableCount.Value);

        if (statuses is { Length: > 0 })
            query = query.Where(ri => statuses.Contains(ri.Status));

        return query.ExecuteUpdateAsync(setters =>
            setters.SetProperty(ri => ri.Status, status),
            cancellationToken
        );
    }
}
