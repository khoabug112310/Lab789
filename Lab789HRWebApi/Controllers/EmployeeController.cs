using Lab789HRWebApi.Caching;
using Lab789HRWebApi.Core.Dtos;
using Lab789HRWebApi.Core.Entities;
using Lab789HRWebApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab789HRWebApi.Controllers
{
    [Route("api/[controller]")]
    [Route("api/Employees")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly HRDbContext context;
        private readonly ICacheService cacheService;
        private const string CacheKey = "employees_list";

        public EmployeeController(HRDbContext context, ICacheService cacheService)
        {
            this.context = context;
            this.cacheService = cacheService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetAll()
        {
            var cachedEmployees = await cacheService.GetAsync<List<Employee>>(CacheKey);
            if (cachedEmployees != null)
            {
                return Ok(cachedEmployees);
            }

            var response = await context.Employees.AsNoTracking().OrderByDescending(e => e.Salary).ToListAsync();
            await cacheService.SetAsync(CacheKey, response, TimeSpan.FromMinutes(5));
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetById(int id)
        {
            var emp = await context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            if (emp == null)
            {
                return NotFound(new { message = "Employee not found." });
            }
            return Ok(emp);
        }

        [HttpPost]
        public async Task<ActionResult<Employee>> Create([FromBody] EmployeeDto dto)
        {
            var empExist = await context.Employees.AnyAsync(e => e.Email == dto.Email);
            if (empExist)
            {
                return Conflict(new { message = "Email đã tồn tại trong hệ thống!" });
            }

            var newEmployee = new Employee
            {
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim(),
                Department = dto.Department.Trim(),
                Position = dto.Position.Trim(),
                Salary = dto.Salary
            };

            await context.Employees.AddAsync(newEmployee);
            await context.SaveChangesAsync();

            // Invalidate Redis cache
            await cacheService.RemoveAsync(CacheKey);

            return Ok(newEmployee);
        }
    }
}
