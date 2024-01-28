namespace Employees.Application
{
    public sealed class UpdatePassport
    {
        public string? Type { get; set; }
        public string? Number { get; set; }

    }
 
    public sealed class UpdateEmployeeDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Phone { get; set; }
        public int? CompanyId { get; set; }
        public UpdatePassport? Passport { get; set; }
        public int? DepartmentId { get; set; }
    }
}
