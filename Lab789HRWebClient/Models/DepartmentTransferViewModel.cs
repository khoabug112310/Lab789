using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Lab789HRWebClient.Models
{
    public class DepartmentTransferItem
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

    public class CreateDepartmentTransferViewModel
    {
        [Required(ErrorMessage = "Please select an employee.")]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Please select or enter the target department.")]
        [Display(Name = "Target Department")]
        [StringLength(100, MinimumLength = 1)]
        public string ToDepartment { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please provide a reason for the transfer.")]
        [Display(Name = "Reason for Transfer")]
        [StringLength(500, MinimumLength = 3)]
        public string Reason { get; set; } = string.Empty;

        public List<SelectListItem> EmployeeList { get; set; } = new();
        public List<SelectListItem> DepartmentList { get; set; } = new();
    }
}
