namespace borrow_nest.Models;

public class CreateCarListingRequest
{
    public string Model { get; set; }
    public int Year { get; set; }
    public int Mileage { get; set; }
    public string Location { get; set; }
    public decimal PricePerDay { get; set; }
    public string Availability { get; set; }  // e.g., "2022-10-01 to 2022-10-15"
    public string PickUpLocation { get; set; }
}