namespace Trainacc.Models
{
    public class Recurrence
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public DateTime NextDate { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}
