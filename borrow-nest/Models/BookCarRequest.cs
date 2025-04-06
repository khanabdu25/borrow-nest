namespace borrow_nest.Models;
public class BookCarRequest
{
    public string CarModel { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int OwnerId { get; set; } // Assuming owner info comes with the request
}