using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab789HRWebApi.Core.Entities
{
    [Table("DepartmentTransferRequests")]
    public class DepartmentTransferRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        [Required]
        [MaxLength(100)]
        public string FromDepartment { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ToDepartment { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // "Pending", "Approved", "Rejected"

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        [MaxLength(200)]
        public string? ReviewedBy { get; set; }

        public DateTime? ReviewDate { get; set; }

        [MaxLength(500)]
        public string? ReviewComment { get; set; }
    }
}
