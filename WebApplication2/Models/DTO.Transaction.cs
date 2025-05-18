using System.ComponentModel.DataAnnotations;

namespace Trainacc.Models
{    public class TransactionDto
    {
        public int Id { get; set; }
        public string? Category { get; set; }
        public decimal TransactionValue { get; set; }
        public DateTime TimeOfTransaction { get; set; }
    }

    public class TransactionSummaryDto
    {
        public int Id { get; set; }
        public string? Category { get; set; }
        public decimal TransactionValue { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class TransactionCreateDto
    {
        [Required] public string? Category { get; set; }
        [Required] public decimal TransactionValue { get; set; }
        [Required] public int RecordId { get; set; }
    }

    public class TransactionUpdateDto
    {
        public string? Category { get; set; }
        public decimal? TransactionValue { get; set; }
    }

    public class TransactionReportDto
    {
        public int TransactionId { get; set; }
        public string? Category { get; set; }
        public decimal Value { get; set; }
        public DateTime Date { get; set; }
    }
}
