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
        return await _db.Properties
            .FirstOrDefaultAsync(p => p.Id == id);
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
        await _db.Properties.Where(p => p.Id == id).ExecuteDeleteAsync();
    }
}
