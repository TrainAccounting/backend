using System.ComponentModel.DataAnnotations;

namespace Trainacc.Models
{    public class RestrictionDto
    {
        public int Id { get; set; }
        public string? Category { get; set; }
        public decimal RestrictionValue { get; set; }
        public decimal MoneySpent { get; set; }
    }

    public class RestrictionCreateDto
    {
        public string? Category { get; set; }
        public decimal RestrictionValue { get; set; }
        public int RecordId { get; set; }
    }

    public class RestrictionUpdateDto
    {
        public string? Category { get; set; }
        public decimal? RestrictionValue { get; set; }
    }
}
