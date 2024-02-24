using DatebaseUpdateEF.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatebaseUpdateEF;

public class ApplicationDBContext : DbContext
{
    public ApplicationDBContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var companyId = Guid.NewGuid();
        modelBuilder.Entity<Company>(builder =>
        {
            builder.HasMany(c => c.Employees).WithOne().HasForeignKey(e => e.CompanyId).IsRequired();

            builder.HasData(new Company()
            {
                Id = companyId,
                Name = "Google",
            });
        });

        modelBuilder.Entity<Employee>(builder =>
        {
            var employees = Enumerable.Range(0, 10000)
                .Select(i => new Employee()
                {
                    Id = Guid.NewGuid(),
                    Name = $"Employee {i}",
                    CompanyId = companyId,
                    Salary = 1000
                }).ToList();
            
            builder.HasData(employees);
        });
    }
}