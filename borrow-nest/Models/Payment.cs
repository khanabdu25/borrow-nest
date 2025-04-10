namespace borrow_nest.Models;

public class Payment
{
    public long ID { get; set; }
    public Listing? Listing { get; set; }

    public Booking? Booking { get; set; }
    public decimal Amount { get; set; }
    public BNUser Recipient { get; set; }

    public BNUser? Sender { get; set; }
}