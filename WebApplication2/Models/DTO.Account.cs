using System.ComponentModel.DataAnnotations;

namespace Trainacc.Models
{    public class AccountDto
    {
        public int Id { get; set; }
        public string? NameOfAccount { get; set; }
        public DateTime DateOfOpening { get; set; }
        public decimal Balance { get; set; }
    }

    public class AccountSummaryDto
    {
        public int Id { get; set; }
        public string? NameOfAccount { get; set; }
        public decimal Balance { get; set; }
    }

    public class AccountCreateDto
    {
        [Required] public string? NameOfAccount { get; set; }
        [Required] public int RecordId { get; set; }
    }

    public class AccountUpdateDto
    {
        public string? NameOfAccount { get; set; }
    }

    public class AccountReportDto
    {
        public string? AccountName { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalValue { get; set; }
    }
}
