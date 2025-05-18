using System.ComponentModel.DataAnnotations;

namespace Trainacc.Models
{
    public class CreditDto
    {
        public int Id { get; set; }
        public string? NameOfCredit { get; set; }
        public decimal CreditCurrentValue { get; set; }
        public DateTime DateOfOpening { get; set; }
        public int PeriodOfPayment { get; set; }
        public decimal InterestRate { get; set; }
        public decimal Amount { get; set; }
        public PaymentType PayType { get; set; } = 0;
        public bool IsActive { get; set; }
    }

    public class CreditCreateDto
    {
        [Required] public string? NameOfCredit { get; set; }
        [Required] public decimal CreditCurrentValue { get; set; }
        [Required] public int PeriodOfPayment { get; set; }
        [Required] public decimal InterestRate { get; set; }
    }

    public class CreditUpdateDto
    {
        public string? NameOfCredit { get; set; }
        public decimal? CreditCurrentValue { get; set; }
        public int? PeriodOfPayment { get; set; }
        public decimal? InterestRate { get; set; }
        public PaymentType PayType { get; set; } = 0;
    }

    public class CreditSummaryDto
    {
        public int AccountId { get; set; }
        public int TotalCredits { get; set; }
        public decimal TotalDebt { get; set; }
        public int ActiveCredits { get; set; }
    }

    public class CreditReportDto
    {
        public int CreditId { get; set; }
        public string? Name { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal InterestRate { get; set; }
        public int PeriodOfPayment { get; set; }
    }
}
