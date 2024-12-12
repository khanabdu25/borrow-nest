namespace borrow_nest.Models;


public class CreateListingRequest
{
    public string Title { get; set; }
    public string Body { get; set; }
    public ProductRequest Product { get; set; }
}

public class ProductRequest
{
    public string Status { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal DailyRate { get; set; }
    public decimal WeeklyRate { get; set; }
    public decimal MonthlyRate { get; set; }
}
