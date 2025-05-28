using System.ComponentModel.DataAnnotations;

namespace Trainacc.Models
{    public class RecordDto
    {
        public int Id { get; set; }
        public string? NameOfRecord { get; set; }
        public DateTime DateOfCreation { get; set; }
        public UserDto? User { get; set; }
        public List<AccountSummaryDto> Accounts { get; set; } = new();
        public List<TransactionSummaryDto> Transactions { get; set; } = new();
        public string? Currency { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal MonthlySalary { get; set; }
        public decimal SavingsGoal { get; set; }
        public string? SavingsGoalName { get; set; }
    }

    public class RecordCreateDto
    {
        [Required]
        public string NameOfRecord { get; set; } = null!;
        public decimal MonthlySalary { get; set; }
        public decimal SavingsGoal { get; set; }
        public string? SavingsGoalName { get; set; }
    }

    public class RecordSummaryDto
    {
        public int Id { get; set; }
        public string? NameOfRecord { get; set; }
        public DateTime DateOfCreation { get; set; }
    }
}
