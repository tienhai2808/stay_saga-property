namespace PropertyService.Models;

public class RoomInventory
{
    public long RoomTypeId { get; set; }
    public DateOnly Date { get; set; }
    public int AvailableCount { get; set; }
    public string Status { get; set; } = RoomInventoryStatuses.Available;
    public RoomType RoomType { get; set; } = null!;
}
