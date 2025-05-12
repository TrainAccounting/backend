using System.ComponentModel.DataAnnotations;

namespace Trainacc.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string? FIO { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string PasswordHash { get; set; } = null!;

        public string? Role { get; set; }
    }

    public class UserAuthDto
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Token { get; set; }
    }

    public class UserCreateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string FIO { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Phone]
        [StringLength(10, MinimumLength = 10)]
        public string? Phone { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        public string? Role { get; set; }
    }

    public class UserUpdateDto
    {
        [StringLength(100)]
        public string? FIO { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = null!;

        public string? Role { get; set; }
    }

    public class UserWithRecordsDto
    {
        public int Id { get; set; }
        public string? FIO { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public List<RecordSummaryDto> Records { get; set; } = new();
    }

    public class RecordDto
    {
        public int Id { get; set; }
        public string? NameOfRecord { get; set; }
        public DateTime DateOfCreation { get; set; }
        public UserDto? User { get; set; }
        public List<AccountSummaryDto> Accounts { get; set; } = new();
        public List<TransactionSummaryDto> Transactions { get; set; } = new();
    }

    public class RecordCreateDto
    {
        [Required]
        public string NameOfRecord { get; set; } = null!;
    }

    public class RecordSummaryDto
    {
        public int Id { get; set; }
        public string? NameOfRecord { get; set; }
        public DateTime DateOfCreation { get; set; }
    }

    public class AccountDto
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

    public class TransactionDto
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

    public class RestrictionDto
    {
        public int Id { get; set; }
        public string? Category { get; set; }
        public decimal RestrictionValue { get; set; }
        public decimal MoneySpent { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

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

    public class UserLoginDto
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }
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

    public class RestrictionCreateDto
    {
        [Required] public string? Category { get; set; }
        [Required] public decimal RestrictionValue { get; set; }
        [Required] public int RecordId { get; set; }
    }

    public class RestrictionUpdateDto
    {
        public string? Category { get; set; }
        public decimal? RestrictionValue { get; set; }
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
        public string Name { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal InterestRate { get; set; }
        public int PeriodOfPayment { get; set; }
    }

    public class AccountReportDto
    {
        public string? AccountName { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class RestrictionSummaryDto
    {
        public int AccountId { get; set; }
        public int TotalRestrictions { get; set; }
        public decimal TotalSpent { get; set; }
        public int ActiveRestrictions { get; set; }
    }

    public class RestrictionReportDto
    {
        public int RestrictionId { get; set; }
        public string? Category { get; set; }
        public decimal RestrictionValue { get; set; }
        public decimal MoneySpent { get; set; }
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

    public class TransactionReportDto
    {
        public int TransactionId { get; set; }
        public string? Category { get; set; }
        public decimal Value { get; set; }
        public DateTime Date { get; set; }
    }

    public class SubscriptionDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}