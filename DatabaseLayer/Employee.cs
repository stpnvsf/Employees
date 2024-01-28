namespace Employees.Persistence
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Phone { get; set; }
        public int CompanyId { get; set; }
        public int PassportId { get; set; }
        public string PassportType { get; set; }
        public string PassportNumber { get; set; }

        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentPhone { get; set; }
    }
}
