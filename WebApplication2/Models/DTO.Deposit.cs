using System.ComponentModel.DataAnnotations;

namespace Trainacc.Models
{
    public class DepositDto
    {
        public int Id { get; set; }
        public string? NameOfDeposit { get; set; }
        public decimal DepositStartValue { get; set; }
        public decimal DepositCurrentValue { get; set; }
        public DateTime DateOfOpening { get; set; }
        public int PeriodOfPayment { get; set; }
        public decimal InterestRate { get; set; }
        public bool Capitalisation { get; set; }
        public decimal Amount { get; set; }
        public PaymentType PayType { get; set; } = 0;
        public bool IsActive { get; set; }
    }

    public class DepositCreateDto
    {
        [Required] public string? NameOfDeposit { get; set; }
        [Required] public decimal DepositStartValue { get; set; }
        [Required] public int PeriodOfPayment { get; set; }
        [Required] public decimal InterestRate { get; set; }
        [Required] public bool Capitalisation { get; set; }
    }

    public class DepositUpdateDto
    {
        public string? NameOfDeposit { get; set; }
        public decimal? DepositCurrentValue { get; set; }
        public int? PeriodOfPayment { get; set; }
        public decimal? InterestRate { get; set; }
        public bool? Capitalisation { get; set; }
        public PaymentType PayType { get; set; } = 0;
    }

    public class DepositSummaryDto
    {
        public int RecordId { get; set; }
        public int TotalDeposits { get; set; }
        public decimal TotalValue { get; set; }
        public int ActiveDeposits { get; set; }
    }

    public class DepositReportDto
    {
        public int DepositId { get; set; }
        public required string Name { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal InterestRate { get; set; }
        public int PeriodOfPayment { get; set; }
    }
}
