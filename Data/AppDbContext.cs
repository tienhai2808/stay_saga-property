using Microsoft.EntityFrameworkCore;
using PropertyService.Models;

namespace PropertyService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<RoomInventory> RoomInventories => Set<RoomInventory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var property = modelBuilder.Entity<Property>();
        property.ToTable("properties");

        property.HasKey(x => x.Id);
        property.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        property.Property(x => x.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnType("character varying(150)");

        property.Property(x => x.Address)
            .HasColumnName("address")
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnType("character varying(150)");

        property.Property(x => x.Ward)
            .HasColumnName("ward")
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnType("character varying(100)");

        property.Property(x => x.City)
            .HasColumnName("city")
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnType("character varying(50)");

        property.Property(x => x.Latitude)
            .HasColumnName("latitude")
            .IsRequired()
            .HasColumnType("double precision");

        property.Property(x => x.Longitude)
            .HasColumnName("longitude")
            .IsRequired()
            .HasColumnType("double precision");

        property.Property(x => x.CheckInTime)
            .HasColumnName("check_in_time")
            .IsRequired()
            .HasColumnType("time without time zone");

        property.Property(x => x.CheckOutTime)
            .HasColumnName("check_out_time")
            .IsRequired()
            .HasColumnType("time without time zone");

        property.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName("ux_properties_name");

        var roomType = modelBuilder.Entity<RoomType>();
        roomType.ToTable("room_types");

        roomType.HasKey(x => x.Id);
        roomType.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        roomType.Property(x => x.PropertyId)
            .HasColumnName("property_id")
            .IsRequired();

        roomType.Property(x => x.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnType("character varying(100)");

        roomType.Property(x => x.Price)
            .HasColumnName("price")
            .IsRequired()
            .HasColumnType("numeric(18,2)");

        roomType.Property(x => x.MaxGuest)
            .HasColumnName("max_guest")
            .IsRequired();

        roomType.Property(x => x.TotalRoom)
            .HasColumnName("total_room")
            .IsRequired();

        roomType.HasOne(x => x.Property)
            .WithMany(x => x.RoomTypes)
            .HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        roomType.HasIndex(x => new { x.PropertyId, x.Name })
            .IsUnique()
            .HasDatabaseName("ux_room_types_property_id_name");

        var roomInventory = modelBuilder.Entity<RoomInventory>();
        roomInventory.ToTable(
            "room_inventories",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_room_inventories_status",
                    $"status IN ('{RoomInventoryStatuses.Available}', '{RoomInventoryStatuses.Booked}', '{RoomInventoryStatuses.Blocked}', '{RoomInventoryStatuses.Maintenance}')"
                );
                table.HasCheckConstraint(
                    "ck_room_inventories_booked_zero",
                    $"status <> '{RoomInventoryStatuses.Booked}' OR available_count = 0"
                );
            });

        roomInventory.HasKey(x => new { x.RoomTypeId, x.Date });

        roomInventory.Property(x => x.RoomTypeId)
            .HasColumnName("room_type_id")
            .IsRequired();

        roomInventory.Property(x => x.Date)
            .HasColumnName("date")
            .IsRequired()
            .HasColumnType("date");

        roomInventory.Property(x => x.AvailableCount)
            .HasColumnName("available_count")
            .IsRequired();

        roomInventory.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnType("character varying(20)");

        roomInventory.HasOne(x => x.RoomType)
            .WithMany(x => x.RoomInventories)
            .HasForeignKey(x => x.RoomTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
