using Microsoft.EntityFrameworkCore;
using Npgsql;
using PropertyService.Data;
using Common.Exceptions;
using PropertyService.Models;

namespace PropertyService.Repositories;

public class PropertyRepository(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task CreateAsync(Property property)
    {
        _db.Properties.Add(property);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException pg &&
            pg.SqlState == PostgresErrorCodes.UniqueViolation
        )
        {
            throw new ConflictException("Property already exists");
        }
    }

    public async Task<Property?> GetByIdAsync(long id)
    {
        return await _db.Properties.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task UpdateAsync(Property property)
    {
        _db.Properties.Update(property);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException pg &&
            pg.SqlState == PostgresErrorCodes.UniqueViolation
        )
        {
            throw new ConflictException("Property already exists");
        }
    }

    public async Task DeleteAsync(long id)
    {
        var affectedRows = await _db.Properties
            .Where(p => p.Id == id)
            .ExecuteDeleteAsync();
        if (affectedRows == 0)
            throw new NotFoundException("Property not found");
    }

    public async Task<(List<Property> Properties, int Total)> ListAsync(
        string search,
        string sort,
        bool isDescending,
        int page,
        int limit
    )
    {
        var query = _db.Properties.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            var pattern = $"%{keyword}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, pattern)
                || EF.Functions.ILike(p.Address, pattern)
                || EF.Functions.ILike(p.Ward, pattern)
                || EF.Functions.ILike(p.City, pattern)
            );
        }

        query = sort switch
        {
            "name" => isDescending
                ? query.OrderByDescending(p => p.Name).ThenByDescending(p => p.Id)
                : query.OrderBy(p => p.Name).ThenBy(p => p.Id),
            "address" => isDescending
                ? query.OrderByDescending(p => p.Address).ThenByDescending(p => p.Id)
                : query.OrderBy(p => p.Address).ThenBy(p => p.Id),
            "ward" => isDescending
                ? query.OrderByDescending(p => p.Ward).ThenByDescending(p => p.Id)
                : query.OrderBy(p => p.Ward).ThenBy(p => p.Id),
            "city" => isDescending
                ? query.OrderByDescending(p => p.City).ThenByDescending(p => p.Id)
                : query.OrderBy(p => p.City).ThenBy(p => p.Id),
            _ => isDescending
                ? query.OrderByDescending(p => p.Id)
                : query.OrderBy(p => p.Id),
        };

        var total = await query.CountAsync();
        var properties = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return (properties, total);
    }
}
