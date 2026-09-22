using Lab789HRWebApi.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lab789HRWebApi.Data
{
    public class HRDbContext : DbContext
    {
        public HRDbContext(DbContextOptions<HRDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<DepartmentTransferRequest> DepartmentTransferRequests => Set<DepartmentTransferRequest>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DepartmentTransferRequest>()
                .HasOne(d => d.Employee)
                .WithMany()
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    Id = 1,
                    FullName = "Alex Tran",
                    Email = "alex@gmail.com",
                    Department = "IT",
                    Position = "Developer",
                    Salary = 1000
                },
                new Employee
                {
                    Id = 2,
                    FullName = "John Doe",
                    Email = "john.doe@gmail.com",
                    Department = "HR",
                    Position = "HR Manager",
                    Salary = 1500
                },
                new Employee
                {
                    Id = 3,
                    FullName = "Jane Smith",
                    Email = "jane.smith@gmail.com",
                    Department = "Finance",
                    Position = "Accountant",
                    Salary = 1200
                }
            );
        }
    }
}
