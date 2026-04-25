namespace PropertyService.Models;

public class RoomType
{
    public long Id { get; set; }
    public long PropertyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int MaxGuest { get; set; }
    public int TotalRoom { get; set; }
    public Property Property { get; set; } = null!;
    public ICollection<RoomInventory> RoomInventories { get; set; } = [];
}
