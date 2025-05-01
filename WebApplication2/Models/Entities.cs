using System.ComponentModel.DataAnnotations;

namespace Trainacc.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string FIO { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<Record> Records { get; set; } = new List<Record>();
    }
    public class Record
    {
        public int Id { get; set; }
        public string NameOfRecord { get; set; }
        public DateTime DateOfCreation { get; set; }
        public int UserId { get; set; }
        public Users User { get; set; }

        public Restriction Restriction { get; set; }
        public Account Account { get; set; }
        public Transactions Transactions { get; set; }
        public Deposit Deposit { get; set; }
        public Credit Credit { get; set; }
    }
    public class Restriction
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public decimal RestrictionValue { get; set; }
        public decimal MoneySpent { get; set; }
        public int RecordId { get; set; }
        public Record Record { get; set; }
    }
    public class Account
    {
        public int Id { get; set; }
        public string NameOfAccount { get; set; }
        public decimal AccountValue { get; set; }
        public DateTime DateOfOpening { get; set; }
        public int RecordId { get; set; }
        public Record Record { get; set; }
    }
    public class Transactions
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public decimal TransactionValue { get; set; }
        public DateTime TimeOfTransaction { get; set; }
        public int RecordId { get; set; }
        public Record Record { get; set; }
    }
    public class Deposit
    {
        public int Id { get; set; }
        public string NameOfDeposit { get; set; }
        public decimal DepositStartValue { get; set; }
        public decimal DepositCurrentValue { get; set; }
        public DateTime DateOfOpening { get; set; }
        public int PeriodOfPayment { get; set; }
        public decimal InterestRate { get; set; }
        public bool Capitalisation { get; set; }
        public PaymentType PayType { get; set; }
        public int RecordId { get; set; }
        public Record Record { get; set; }
    }
    public class Credit
    {
        public int Id { get; set; }
        public string NameOfCredit { get; set; }
        public decimal CreditCurrentValue { get; set; }
        public DateTime DateOfOpening { get; set; }
        public int PeriodOfPayment { get; set; }
        public decimal InterestRate { get; set; }
        public PaymentType PayType { get; set; }
        public int RecordId { get; set; }
        public Record Record { get; set; }
    }

    public enum PaymentType
    {
        Monthly,
        Quarterly,
        Once
    }
}