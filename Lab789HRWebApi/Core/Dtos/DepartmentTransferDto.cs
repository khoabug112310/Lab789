using System.ComponentModel.DataAnnotations;

namespace Lab789HRWebApi.Core.Dtos
{
    public class CreateDepartmentTransferDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string ToDepartment { get; set; } = string.Empty;

        [Required]
        [StringLength(500, MinimumLength = 3)]
        public string Reason { get; set; } = string.Empty;
    }

    public class ReviewDepartmentTransferDto
    {
        public bool IsApproved { get; set; }

        public string? ReviewComment { get; set; }
    }

    public class DepartmentTransferResponseDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public string FromDepartment { get; set; } = string.Empty;
        public string ToDepartment { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string? ReviewComment { get; set; }
    }
}
