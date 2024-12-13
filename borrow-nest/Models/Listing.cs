namespace borrow_nest.Models;

public class Listing
{
    public long ID { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public required BNUser BNUser { get; set; }
    public Product Product { get; set; }
    public ListingStatus Status { get; set; } = ListingStatus.AVAILABLE;
}

public enum ListingStatus
{
    AVAILABLE,
    CLAIMED
}