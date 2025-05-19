using System.ComponentModel.DataAnnotations;

namespace Trainacc.Models
{
    public class SubscriptionDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Amount { get; set; }
        public string? Category { get; set; }
        public TransactionType Type { get; set; }
        public int RecordId { get; set; }
        public DateTime StartDate { get; set; }
        public int PeriodDays { get; set; }
        public bool IsActive { get; set; }
    }

    public class SubscriptionCreateDto
    {
        [Required] public string? Name { get; set; }
        [Required] public decimal Amount { get; set; }
        [Required] public string? Category { get; set; }
        [Required] public TransactionType Type { get; set; }
        [Required] public int RecordId { get; set; }
        [Required] public DateTime StartDate { get; set; }
        [Required] public int PeriodDays { get; set; }
    }
}
