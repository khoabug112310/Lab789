using Lab789HRWebApi.Caching;
using Lab789HRWebApi.Core.Dtos;
using Lab789HRWebApi.Core.Entities;
using Lab789HRWebApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab789HRWebApi.Controllers
{
    [Route("api/DepartmentTransfers")]
    [ApiController]
    public class DepartmentTransferController : ControllerBase
    {
        private readonly HRDbContext context;
        private readonly ICacheService cacheService;
        private const string CacheKeyPrefix = "transfer_requests_";
        private const string EmployeeCacheKey = "employees_list";

        public DepartmentTransferController(HRDbContext context, ICacheService cacheService)
        {
            this.context = context;
            this.cacheService = cacheService;
        }

        // GET: api/DepartmentTransfers?status=Pending
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentTransferResponseDto>>> GetAll([FromQuery] string? status = null)
        {
            var cacheKey = $"{CacheKeyPrefix}{status ?? "all"}";
            var cached = await cacheService.GetAsync<List<DepartmentTransferResponseDto>>(cacheKey);
            if (cached != null)
            {
                return Ok(cached);
            }

            var query = context.DepartmentTransferRequests.Include(r => r.Employee).AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.Status == status);
            }

            var list = await query.OrderByDescending(r => r.RequestDate)
                .Select(r => new DepartmentTransferResponseDto
                {
                    Id = r.Id,
                    EmployeeId = r.EmployeeId,
                    EmployeeName = r.Employee != null ? r.Employee.FullName : "Unknown",
                    EmployeeEmail = r.Employee != null ? r.Employee.Email : "Unknown",
                    FromDepartment = r.FromDepartment,
                    ToDepartment = r.ToDepartment,
                    Reason = r.Reason,
                    Status = r.Status,
                    RequestDate = r.RequestDate,
                    ReviewedBy = r.ReviewedBy,
                    ReviewDate = r.ReviewDate,
                    ReviewComment = r.ReviewComment
                }).ToListAsync();

            await cacheService.SetAsync(cacheKey, list, TimeSpan.FromMinutes(5));
            return Ok(list);
        }

        // GET: api/DepartmentTransfers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentTransferResponseDto>> GetById(int id)
        {
            var r = await context.DepartmentTransferRequests.Include(x => x.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (r == null)
            {
                return NotFound(new { message = "Transfer request not found." });
            }

            return Ok(new DepartmentTransferResponseDto
            {
                Id = r.Id,
                EmployeeId = r.EmployeeId,
                EmployeeName = r.Employee != null ? r.Employee.FullName : "Unknown",
                EmployeeEmail = r.Employee != null ? r.Employee.Email : "Unknown",
                FromDepartment = r.FromDepartment,
                ToDepartment = r.ToDepartment,
                Reason = r.Reason,
                Status = r.Status,
                RequestDate = r.RequestDate,
                ReviewedBy = r.ReviewedBy,
                ReviewDate = r.ReviewDate,
                ReviewComment = r.ReviewComment
            });
        }

        // GET: api/DepartmentTransfers/employee/1
        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<DepartmentTransferResponseDto>>> GetByEmployee(int employeeId)
        {
            var list = await context.DepartmentTransferRequests.Include(x => x.Employee)
                .AsNoTracking()
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.RequestDate)
                .Select(r => new DepartmentTransferResponseDto
                {
                    Id = r.Id,
                    EmployeeId = r.EmployeeId,
                    EmployeeName = r.Employee != null ? r.Employee.FullName : "Unknown",
                    EmployeeEmail = r.Employee != null ? r.Employee.Email : "Unknown",
                    FromDepartment = r.FromDepartment,
                    ToDepartment = r.ToDepartment,
                    Reason = r.Reason,
                    Status = r.Status,
                    RequestDate = r.RequestDate,
                    ReviewedBy = r.ReviewedBy,
                    ReviewDate = r.ReviewDate,
                    ReviewComment = r.ReviewComment
                }).ToListAsync();

            return Ok(list);
        }

        // POST: api/DepartmentTransfers
        [HttpPost]
        public async Task<ActionResult<DepartmentTransferResponseDto>> Create([FromBody] CreateDepartmentTransferDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var employee = await context.Employees.FirstOrDefaultAsync(e => e.Id == dto.EmployeeId);
            if (employee == null)
            {
                return NotFound(new { message = $"Employee with ID {dto.EmployeeId} not found." });
            }

            var targetDept = dto.ToDepartment.Trim();
            if (string.Equals(employee.Department, targetDept, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = $"Employee is already in department '{targetDept}'." });
            }

            var hasPending = await context.DepartmentTransferRequests
                .AnyAsync(r => r.EmployeeId == dto.EmployeeId && r.Status == "Pending");
            if (hasPending)
            {
                return Conflict(new { message = "This employee already has a pending transfer request." });
            }

            var request = new DepartmentTransferRequest
            {
                EmployeeId = employee.Id,
                FromDepartment = employee.Department,
                ToDepartment = targetDept,
                Reason = dto.Reason.Trim(),
                Status = "Pending",
                RequestDate = DateTime.UtcNow
            };

            await context.DepartmentTransferRequests.AddAsync(request);
            await context.SaveChangesAsync();

            await InvalidateTransferCachesAsync();

            return Ok(new DepartmentTransferResponseDto
            {
                Id = request.Id,
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                EmployeeEmail = employee.Email,
                FromDepartment = request.FromDepartment,
                ToDepartment = request.ToDepartment,
                Reason = request.Reason,
                Status = request.Status,
                RequestDate = request.RequestDate
            });
        }

        // POST: api/DepartmentTransfers/5/approve
        [HttpPost("{id}/approve")]
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(int id, [FromBody] ReviewDepartmentTransferDto? reviewDto = null)
        {
            var request = await context.DepartmentTransferRequests.Include(r => r.Employee)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound(new { message = "Transfer request not found." });
            }

            if (request.Status != "Pending")
            {
                return BadRequest(new { message = $"Cannot approve a request with status '{request.Status}'." });
            }

            var reviewer = User.Identity?.Name ?? "Manager";

            request.Status = "Approved";
            request.ReviewedBy = reviewer;
            request.ReviewDate = DateTime.UtcNow;
            request.ReviewComment = string.IsNullOrWhiteSpace(reviewDto?.ReviewComment) 
                ? "Approved by Manager" 
                : reviewDto.ReviewComment.Trim();

            // CRUCIAL: Update employee's department
            if (request.Employee != null)
            {
                request.Employee.Department = request.ToDepartment;
            }
            else
            {
                var emp = await context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId);
                if (emp != null)
                {
                    emp.Department = request.ToDepartment;
                }
            }

            await context.SaveChangesAsync();

            // Invalidate Redis caches
            await InvalidateTransferCachesAsync();
            await cacheService.RemoveAsync(EmployeeCacheKey);

            return Ok(new
            {
                message = "Department transfer request approved successfully.",
                requestId = request.Id,
                employeeId = request.EmployeeId,
                newDepartment = request.ToDepartment,
                status = request.Status,
                reviewDate = request.ReviewDate,
                reviewedBy = request.ReviewedBy
            });
        }

        // POST: api/DepartmentTransfers/5/reject
        [HttpPost("{id}/reject")]
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] ReviewDepartmentTransferDto? reviewDto = null)
        {
            var request = await context.DepartmentTransferRequests.FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound(new { message = "Transfer request not found." });
            }

            if (request.Status != "Pending")
            {
                return BadRequest(new { message = $"Cannot reject a request with status '{request.Status}'." });
            }

            var reviewer = User.Identity?.Name ?? "Manager";

            request.Status = "Rejected";
            request.ReviewedBy = reviewer;
            request.ReviewDate = DateTime.UtcNow;
            request.ReviewComment = string.IsNullOrWhiteSpace(reviewDto?.ReviewComment) 
                ? "Rejected by Manager" 
                : reviewDto.ReviewComment.Trim();

            await context.SaveChangesAsync();

            await InvalidateTransferCachesAsync();

            return Ok(new
            {
                message = "Department transfer request rejected.",
                requestId = request.Id,
                status = request.Status,
                reviewDate = request.ReviewDate,
                reviewedBy = request.ReviewedBy,
                comment = request.ReviewComment
            });
        }

        private async Task InvalidateTransferCachesAsync()
        {
            await cacheService.RemoveAsync($"{CacheKeyPrefix}all");
            await cacheService.RemoveAsync($"{CacheKeyPrefix}Pending");
            await cacheService.RemoveAsync($"{CacheKeyPrefix}Approved");
            await cacheService.RemoveAsync($"{CacheKeyPrefix}Rejected");
        }
    }
}
