using System.ComponentModel.DataAnnotations;

namespace Trainacc.Models
{
    // DTO для записей (Records)
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
}
