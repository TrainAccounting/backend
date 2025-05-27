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
        public int RecordId { get; set; }
    }

    public class CreditCreateDto
    {
        public string? NameOfCredit { get; set; }
        public decimal CreditCurrentValue { get; set; }
        public int PeriodOfPayment { get; set; }
        public decimal InterestRate { get; set; }
    }

    public class CreditUpdateDto
    {
        public string? NameOfCredit { get; set; }
        public decimal? CreditCurrentValue { get; set; }
        public int? PeriodOfPayment { get; set; }
        public decimal? InterestRate { get; set; }
    }
}
