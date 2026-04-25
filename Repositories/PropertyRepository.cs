using Microsoft.EntityFrameworkCore;
using Npgsql;
using PropertyService.Data;
using PropertyService.Exceptions;
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
}