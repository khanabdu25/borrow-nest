namespace borrow_nest.Services
{
    using borrow_nest.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;

    public class BalanceService
    {
        private readonly BNContext _context;

        public BalanceService(BNContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetUserBalanceAsync(string userId)
        {
            // Get the total amount the user has received (positive amounts as recipient)
            var receivedAmount = await _context.Payments
                .Where(p => p.Recipient.Id == userId && p.Amount > 0)
                .SumAsync(p => p.Amount);

            Console.WriteLine($"Received Amount: {receivedAmount}");

            // Get the total amount the user has sent (negative amounts as sender)
            var sentAmount = await _context.Payments
                .Where(p => p.Sender.Id == userId && p.Amount < 0)
                .SumAsync(p => p.Amount);

            Console.WriteLine($"Sent Amount: {sentAmount}");

            // The balance is the total received minus the total sent (expenses)
            return receivedAmount + sentAmount;  // Sent amounts are negative, so this effectively subtracts
        }
    }
}
