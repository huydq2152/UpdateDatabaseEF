namespace DatebaseUpdateEF.Entities;

public class Employee
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Salary { get; set; }
    public Guid CompanyId { get; set; }
}