using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trainacc.Models
{    public class TransactionDto
    {
        public int Id { get; set; }
        public string? Category { get; set; }
        public decimal TransactionValue { get; set; }
        public DateTime TimeOfTransaction { get; set; }
        public TransactionType Type { get; set; }
        public int IsPlannedInt { get; set; } = 0;
        [NotMapped]
        public bool IsPlanned
        {
            get => IsPlannedInt == 1;
            set => IsPlannedInt = value ? 1 : 0;
        }
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
        public string? Category { get; set; }

        [Required]
        public decimal TransactionValue { get; set; }

        [Required]
        public int RecordId { get; set; }

        [Required]
        public TransactionType Type { get; set; }
        public string? Description { get; set; }
        [Required]
        [Range(0, 1, ErrorMessage = "isPlanned должен быть 0 (false) или 1 (true)")]
        public int IsPlannedInt { get; set; } = 0;
        [NotMapped]
        public bool IsPlanned
        {
            get => IsPlannedInt == 1;
            set => IsPlannedInt = value ? 1 : 0;
        }
        public string? PlannedDate { get; set; }
    }

    public class TransactionUpdateDto
    {
        public string? Category { get; set; }
        public decimal? TransactionValue { get; set; }
        public TransactionType? Type { get; set; }
        public string? Description { get; set; }
        public int? IsPlannedInt { get; set; } = 0;
        [NotMapped]
        public bool? IsPlanned
        {
            get => IsPlannedInt == 1;
            set => IsPlannedInt = value == true ? 1 : 0;
        }
        public string? PlannedDate { get; set; }
    }

    public class TransactionReportDto
    {
        public int TransactionId { get; set; }
        public string? Category { get; set; }
        public decimal Value { get; set; }
        public DateTime Date { get; set; }
    }
}
