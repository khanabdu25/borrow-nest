namespace borrow_nest.Models;

public class SearchCriteria
{
    public string Location { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? MaxPrice { get; set; }
}