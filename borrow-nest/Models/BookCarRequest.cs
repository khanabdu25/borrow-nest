namespace borrow_nest.Models;
public class BookCarRequest
{
    public int ListingId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string OwnerId { get; set; }
}