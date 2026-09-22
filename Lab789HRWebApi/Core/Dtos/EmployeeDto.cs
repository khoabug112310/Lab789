using System.ComponentModel.DataAnnotations;

namespace Lab789HRWebApi.Core.Dtos
{
    public class EmployeeDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Position { get; set; } = string.Empty;

        [Range(0, 100000)]
        public decimal Salary { get; set; }
    }
}
