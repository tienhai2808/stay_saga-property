using Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PropertyService.Data;
using PropertyService.Models;

namespace PropertyService.Repositories;

public class RoomTypeRepository(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task CreateAsync(RoomType roomType, CancellationToken cancellationToken = default)
    {
        _db.RoomTypes.Add(roomType);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
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

    public Task<RoomType?> FindByIdAsync(long id, CancellationToken cancellationToken = default)
        => _db.RoomTypes.FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);

    public Task<RoomType?> FindByIdWithPropertyAsync(long id, CancellationToken cancellationToken = default)
        => _db.RoomTypes
            .Include(rt => rt.Property)
            .FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);

    public async Task UpdateAsync(RoomType roomType, CancellationToken cancellationToken = default)
    {
        _db.RoomTypes.Update(roomType);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException pg &&
            pg.SqlState == PostgresErrorCodes.ForeignKeyViolation
        )
        {
            throw new NotFoundException("Property not found");
        }
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var affectedRows = await _db.RoomTypes
            .Where(rt => rt.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
        if (affectedRows == 0)
            throw new NotFoundException("Room type not found");
    }

    public async Task<(List<RoomType>, int)> FindAllByPropertyIdAsync(
        long propertyId,
        string search,
        string sort,
        bool isDescending,
        int page,
        int limit,
        CancellationToken cancellationToken = default
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

        var total = await query.CountAsync(cancellationToken);
        var roomTypes = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return (roomTypes, total);
    }

    public async Task<(List<RoomType>, int)> FindAllWithPropertyAsync(
        long? propertyId,
        string search,
        string sort,
        bool isDescending,
        int page,
        int limit,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<RoomType> query = _db.RoomTypes
            .AsNoTracking()
            .Include(rt => rt.Property);

        if (propertyId.HasValue)
            query = query.Where(rt => rt.PropertyId == propertyId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            var pattern = $"%{keyword}%";
            query = query.Where(rt => EF.Functions.ILike(rt.Name, pattern));
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

        var total = await query.CountAsync(cancellationToken);
        var roomTypes = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return (roomTypes, total);
    }
}
