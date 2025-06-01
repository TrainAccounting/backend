using System.ComponentModel.DataAnnotations;

namespace Trainacc.Models
{    public class UserDto
    {
        public int Id { get; set; }
        public string? FIO { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string PasswordHash { get; set; } = null!;
        public string? Role { get; set; }
        public string? RealPassword { get; set; }
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
        [StringLength(100, MinimumLength = 1)]
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

    public class UserLoginDto
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
