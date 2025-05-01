namespace Trainacc.Models
{
    public class Deposit
    {
        public int Id { get; set; }
        public decimal Value { get; set; }
        public DateTime OpenedDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; } // смысл ответа
    }
}
