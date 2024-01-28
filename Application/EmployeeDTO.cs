namespace Employees.Application
{
    public sealed class Passport
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }

    }
    public sealed class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
    }
    public sealed class EmployeeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Phone { get; set; }
        public int CompanyId { get; set; }
        public Passport Passport { get; set; }
        public Department Department { get; set; }
    }
}
