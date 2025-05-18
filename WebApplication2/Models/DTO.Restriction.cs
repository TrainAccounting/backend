using System.ComponentModel.DataAnnotations;

namespace Trainacc.Models
{    public class RestrictionDto
    {
        public int Id { get; set; }
        public string? Category { get; set; }
        public decimal RestrictionValue { get; set; }
        public decimal MoneySpent { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
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
}
