namespace Trainacc.Models
{
    public class Credit
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Amount { get; set; }
        public DateTime OpenedDate { get; set; } = DateTime.UtcNow;
        public decimal InterestRate { get; set; } // ставка
        public decimal Penalty { get; set; } // штраф
    }
}
