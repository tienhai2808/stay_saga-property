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
}
