namespace Employees.Application
{
    public interface IRepository
    {
        Task<EmployeeDTO> GetById(int id);
        Task<IEnumerable<EmployeeDTO>> GetEmployeesByCompanyAsync(int company);
        Task<IEnumerable<EmployeeDTO>> GetEmployeesByDepartmentAsync(int company, int department);
        Task<int> AddEmployeeAsync(CreateEmployeeDTO employee);
        Task<int> DeleteEmployee(int id);
        Task<int> ChangeEmployeeAsync(EmployeeDTO employee);
    }
}
