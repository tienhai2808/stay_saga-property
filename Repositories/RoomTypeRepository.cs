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

    public async Task<(List<RoomType> RoomTypes, int Total)> ListByPropertyIdAsync(
        long propertyId,
        string search,
        string sort,
        bool isDescending,
        int page,
        int limit
    )
    {
        var query = _db.RoomTypes.AsNoTracking().Where(rt => rt.PropertyId == propertyId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            var pattern = $"%{keyword}%";
            query = query.Where(rt =>
                EF.Functions.ILike(rt.Name, pattern)
            );
        }

        query = sort switch
        {
            "name" => isDescending
                ? query.OrderByDescending(rt => rt.Name).ThenByDescending(rt => rt.Id)
                : query.OrderBy(rt => rt.Name).ThenBy(rt => rt.Id),
            "price" => isDescending
                ? query.OrderByDescending(rt => rt.Price).ThenByDescending(rt => rt.Id)
                : query.OrderBy(rt => rt.Price).ThenBy(rt => rt.Id),
            _ => isDescending
                ? query.OrderByDescending(rt => rt.Id)
                : query.OrderBy(rt => rt.Id),
        };

        var total = await query.CountAsync();
        var roomTypes = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return (roomTypes, total);
    }
}
