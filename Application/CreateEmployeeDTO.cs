namespace Employees.Application
{
    public sealed class CreatePassportDTO
    {
        public string Type { get; set; }
        public string Number { get; set; }
    }
    public sealed class CreateEmployeeDTO
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Phone { get; set; }
        public int CompanyId { get; set; }
        public CreatePassportDTO Passport { get; set; }
        public int DepartmentId { get; set; }
    }
}
