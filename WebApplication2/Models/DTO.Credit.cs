using System.ComponentModel.DataAnnotations;

namespace Trainacc.Models
{
    public class CreditDto
    {
        public int Id { get; set; }
        public string? NameOfCredit { get; set; }
        public decimal CreditStartValue { get; set; }
        public decimal CreditCurrentValue { get; set; } 
        public DateTime DateOfOpening { get; set; }
        public int PeriodOfPayment { get; set; }
        public decimal InterestRate { get; set; }
        public decimal InterestRateInMoney { get; set; }
        public bool IsOver { get; set; }
        public decimal TotalToPay { get; set; }
        public decimal MonthlyPayment { get; set; }
        public bool IsEnoughMoney { get; set; }
        public int AccountId { get; set; }
        public int TogetAccountId { get; set; } 
    }

    public class CreditCreateDto
    {
        public string? NameOfCredit { get; set; }
        public decimal CreditStartValue { get; set; }
        public decimal CreditCurrentValue { get; set; } 
        public int PeriodOfPayment { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime? DateOfOpening { get; set; }
    }

    public class CreditUpdateDto
    {
        public string? NameOfCredit { get; set; }
        public int? AccountId { get; set; }
        public int? PeriodOfPayment { get; set; }
        public decimal? InterestRate { get; set; }
        public decimal? CreditCurrentValue { get; set; } 
        public int? TogetAccountId { get; set; }
    }
}
