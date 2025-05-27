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
        public decimal Amount { get; set; }
        public int RecordId { get; set; }
    }

    public class DepositCreateDto
    {
        public string? NameOfDeposit { get; set; }
        public decimal DepositStartValue { get; set; }
        public int PeriodOfPayment { get; set; }
        public decimal InterestRate { get; set; }
    }

    public class DepositUpdateDto
    {
        public string? NameOfDeposit { get; set; }
        public decimal? DepositCurrentValue { get; set; }
        public int? PeriodOfPayment { get; set; }
        public decimal? InterestRate { get; set; }
    }
}
