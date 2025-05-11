using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Trainacc.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string? FIO { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        [Required]
        public string PasswordHash { get; set; } = null!;
        [Required]
        public string Role { get; set; } = "User";
        public List<Record> Records { get; set; } = new List<Record>();
    }
    public class Record
    {
        public int Id { get; set; }
        public string? NameOfRecord { get; set; }
        public DateTime DateOfCreation { get; set; }
        public int UserId { get; set; }
        public Users? User { get; set; }

        public List<Restriction> Restrictions { get; set; } = new List<Restriction>();
        public List<Account> Accounts { get; set; } = new List<Account>();
        public List<Transactions> Transactions { get; set; } = new List<Transactions>();
        public List<Deposit> Deposits { get; set; } = new List<Deposit>();
        public List<Credit> Credits { get; set; } = new List<Credit>();
    }
    public class Restriction
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Category { get; set; }
        public decimal RestrictionValue { get; set; }
        public decimal MoneySpent { get; set; }
        public int RecordId { get; set; }
        public string Name { get; set; } 
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
    public class Account
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? NameOfAccount { get; set; }
        public decimal AccountValue { get; set; }
        public DateTime DateOfOpening { get; set; }
        public int RecordId { get; set; } 
        public ICollection<Transactions> Transactions { get; set; }
        public decimal Balance { get; set; }
    }
    public class Transactions
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Category { get; set; }
        public decimal TransactionValue { get; set; }
        public DateTime TimeOfTransaction { get; set; }
        public int RecordId { get; set; }
    }
    public class Deposit
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? NameOfDeposit { get; set; }
        public decimal DepositStartValue { get; set; }
        public decimal DepositCurrentValue { get; set; }
        public DateTime DateOfOpening { get; set; }
        public int PeriodOfPayment { get; set; }
        public decimal InterestRate { get; set; }
        public bool Capitalisation { get; set; }
        public PaymentType PayType { get; set; }
        public decimal Amount { get; set; }
        public int RecordId { get; set; }
        public bool IsActive { get; set; }
    }
    public class Credit
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? NameOfCredit { get; set; }
        public decimal CreditCurrentValue { get; set; }
        public DateTime DateOfOpening { get; set; }
        public int PeriodOfPayment { get; set; }
        public decimal InterestRate { get; set; }
        public PaymentType PayType { get; set; }
        public decimal Amount { get; set; }
        public int RecordId { get; set; }
        public bool IsActive { get; set; }
    }

    public enum PaymentType
    {
        Monthly
    }
}