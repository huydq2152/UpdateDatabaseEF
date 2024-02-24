using Dapper;
using DatebaseUpdateEF;
using DatebaseUpdateEF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPut("update-salaries-by-company-basic", async (Guid companyId, ApplicationDBContext dbContext) =>
{
    // Why save change is slow : https://code-maze.com/efcore-bulk-updates/
    
    var company = await dbContext.Set<Company>()
        .Include(o => o.Employees)
        .AsSplitQuery()
        .FirstOrDefaultAsync(o => o.Id == companyId);

    if (company is null)
    {
        return Results.NotFound($"Not found company with id {companyId}");
    }

    foreach (var employee in company.Employees)
    {
        employee.Salary *= 1.1m;
    }

    company.UpdateSalaryDate = DateTime.Now;
    await dbContext.SaveChangesAsync();
    return Results.Ok($"Updated salaries for company {company.Name}");
});

app.MapPut("update-salaries-by-company-sql-raw", async (Guid companyId, ApplicationDBContext dbContext) =>
{
    var company = await dbContext.Set<Company>().FirstOrDefaultAsync(o => o.Id == companyId);
    if (company is null)
    {
        return Results.NotFound($"Not found company with id {companyId}");
    }

    FormattableString query = $@"
        UPDATE Employee
        SET Salary = Salary * 1.1
        WHERE CompanyId = {companyId}";

    // Execute the SQL query and EF core in the same transaction to ensure consistency
    await using (var transaction = await dbContext.Database.BeginTransactionAsync())
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync(query);
        company.UpdateSalaryDate = DateTime.Now;
        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }


    return Results.Ok($"Updated salaries for company {company.Name}");
});

app.MapPut("update-salaries-by-company-dapper", async (Guid companyId, ApplicationDBContext dbContext) =>
{
    var company = await dbContext.Set<Company>().FirstOrDefaultAsync(o => o.Id == companyId);
    if (company is null)
    {
        return Results.NotFound($"Not found company with id {companyId}");
    }

    var query = $@"
        UPDATE Employee
        SET Salary = Salary * 1.1
        WHERE CompanyId = @CompanyId";

    // Execute dapper and EF core in the same transaction to ensure consistency

    await using (var transaction = await dbContext.Database.BeginTransactionAsync())
    {
        await dbContext.Database.GetDbConnection()
            .ExecuteAsync(query, new { CompanyId = companyId }, transaction.GetDbTransaction());
        company.UpdateSalaryDate = DateTime.Now;
        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    return Results.Ok($"Updated salaries for company {company.Name}");
});

app.MapPut("update-salaries-by-company-ef-core-bulk-extension",
    async (Guid companyId, ApplicationDBContext dbContext) =>
    {
        var company = await dbContext.Set<Company>().FirstOrDefaultAsync(o => o.Id == companyId);
        if (company is null)
        {
            return Results.NotFound($"Not found company with id {companyId}");
        }

        var employees = await dbContext.Set<Employee>().Where(o => o.CompanyId == companyId).ToListAsync();
        foreach (var employee in employees)
        {
            employee.Salary *= 1.1m;
        }
        
        await using (var transaction = await dbContext.Database.BeginTransactionAsync())
        {
            await dbContext.BulkUpdateAsync(employees);
            company.UpdateSalaryDate = DateTime.Now;
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        return Results.Ok($"Updated salaries for company {company.Name}");
    });

app.MapPut("update-salaries-by-company-ef-core-bulk-after-version-7",
    async (Guid companyId, ApplicationDBContext dbContext) =>
    {
        var company = await dbContext.Set<Company>().FirstOrDefaultAsync(o => o.Id == companyId);
        if (company is null)
        {
            return Results.NotFound($"Not found company with id {companyId}");
        }

        await using (var transaction = await dbContext.Database.BeginTransactionAsync())
        {
            await dbContext.Set<Employee>().Where(o=>o.CompanyId == companyId)
                .ExecuteUpdateAsync(s=>s.SetProperty(o=>o.Salary, o=>o.Salary * 1.1m));
            company.UpdateSalaryDate = DateTime.Now;
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        return Results.Ok($"Updated salaries for company {company.Name}");
    });

app.Run();