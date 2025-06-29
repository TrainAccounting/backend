using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trainacc.Models
{    public class TransactionDto
    {
        public int Id { get; set; }
        public string? Category { get; set; }
        public decimal TransactionValue { get; set; }
        public DateTime TimeOfTransaction { get; set; }
        public bool IsAdd { get; set; }
    }

    public class TransactionSummaryDto
    {
        public string? Category { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalValue { get; set; }
        public TransactionType Type { get; set; }
    }

    public class TransactionCreateDto
    {
        [Required]
        public bool IsAdd { get; set; }
        [Required]
        public string? Category { get; set; }

        [Required]
        public decimal TransactionValue { get; set; }

        public string? Description { get; set; }
    }

    public class TransactionUpdateDto
    {
        public string? Category { get; set; }
        public decimal? TransactionValue { get; set; }
        public bool? IsAdd { get; set; }
        public string? Description { get; set; }
    }

    public class TransactionReportDto
    {
        public int TransactionId { get; set; }
        public string? Category { get; set; }
        public decimal Value { get; set; }
        public DateTime Date { get; set; }
    }
}
