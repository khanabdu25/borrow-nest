namespace borrow_nest.Models;

public class Product
{
    public long ID { get; set; }
    public string? Status { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal DailyRate { get; set; }
    public decimal WeeklyRate { get; set; }
    public decimal MonthlyRate { get; set; }
}