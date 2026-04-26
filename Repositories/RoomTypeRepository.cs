using Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PropertyService.Data;
using PropertyService.Models;

namespace PropertyService.Repositories;

public class RoomTypeRepository(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task CreateAsync(RoomType roomType)
    {
        _db.RoomTypes.Add(roomType);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException pg &&
            pg.SqlState == PostgresErrorCodes.UniqueViolation
        )
        {
            throw new ConflictException("Room type already exists in this property");
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException pg &&
            pg.SqlState == PostgresErrorCodes.ForeignKeyViolation
        )
        {
            throw new NotFoundException("Property not found");
        }
    }
}
