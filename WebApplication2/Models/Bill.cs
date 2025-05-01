namespace Trainacc.Models
{
    public class Bill
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(30);
    }
}
