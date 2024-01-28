using Dapper;
using System.Data;
using System.Data.SqlClient;
using Employees.Application;
using Microsoft.Extensions.Configuration;

namespace Employees.Persistence
{
    public sealed class Repository : IRepository
    {

        private readonly string _connectionString;

        public Repository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Database");
        }

        public async Task<int> AddEmployeeAsync(CreateEmployeeDTO employee)
        {

            await using (var db = new SqlConnection(_connectionString))
            {
                db.Open();

                await using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        var departmentExists = await DepartmentExists(employee.DepartmentId);
                        if (!departmentExists)
                        {
                            throw new KeyNotFoundException($"Department with id = {employee.DepartmentId} not found");
                        }

                        var passportParameters = new { employee.Passport.Type, employee.Passport.Number };
                        var passportInsert = """
                        INSERT INTO Passports (Type, Number)
                        OUTPUT INSERTED.Id
                        VALUES (@Type, @Number);
                        """;
                        var passportId = await db.QuerySingleAsync<int>(passportInsert, passportParameters, transaction: transaction);

                        var employeeParameters = new
                        {
                            Name = employee.Name,
                            Surname = employee.Surname,
                            Phone = employee.Phone,
                            CompanyId = employee.CompanyId,
                            PassportId = passportId,
                            DepartmentId = employee.DepartmentId
                        };
                        var employeeInsert = """
                         INSERT INTO Employees (
                             Name,
                             Surname,
                             Phone,
                             CompanyId,
                             PassportId,
                             DepartmentId)
                         OUTPUT INSERTED.Id
                         VALUES (
                             @Name,
                             @Surname,
                             @Phone,
                             @CompanyId,
                             @PassportId,
                             @DepartmentId
                         )
                         """;
                        var employeeId = await db.QuerySingleAsync<int>(employeeInsert, employeeParameters, transaction: transaction);

                        await transaction.CommitAsync();
                        return employeeId;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<int> ChangeEmployeeAsync(EmployeeDTO employee)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                db.Open();

                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        if (!(await DepartmentExists(employee.Department.Id)))
                        {
                            throw new KeyNotFoundException($"Department with id = {employee.Department.Id} not found");
                        }

                        var passportQuery = $"""
                            UPDATE Passports
                            SET Type = @Type,
                            Number = @Number
                            WHERE Id = @Id
                            """;
                        await db.ExecuteAsync(passportQuery, new { Id = employee.Passport.Id, Type = employee.Passport.Type, Number = employee.Passport.Number }, transaction: transaction);

                        var employeeParameters = new
                        {
                            Id = employee.Id,
                            Name = employee.Name,
                            Surname = employee.Surname,
                            Phone = employee.Phone,
                            CompanyId = employee.CompanyId,
                            PassportId = employee.Passport.Id,
                            DepartmentId = employee.Department.Id
                        };

                        string query = $"""
                            UPDATE Employees
                            SET Name = @Name,
                            Surname = @Surname,
                            Phone = @Phone,
                            CompanyId = @CompanyId,
                            PassportId = @PassportId,
                            DepartmentId = @DepartmentId
                            WHERE Id = @Id;
                            """;
                        var result = await db.ExecuteAsync(query, employeeParameters, transaction: transaction);

                        transaction.Commit();
                        return result;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<int> DeleteEmployee(int id)
        {
            await using (var db = new SqlConnection(_connectionString))
            {
                db.Open();

                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        var passportId = await db.QueryFirstOrDefaultAsync<int>("""
                            SELECT PassportId
                            FROM Employees
                            WHERE Id = @Id
                            """,
                            new { Id = id }, transaction: transaction);

                        string employeeQuery = """
                            DELETE FROM Employees
                            WHERE Id = @Id;
                            """;
                        await db.ExecuteAsync(employeeQuery, new { Id = id }, transaction: transaction);

                        string query = """
                            DELETE FROM Passports
                            WHERE Id = @Id;
                            """;
                        var result = await db.ExecuteAsync(query, new { Id = passportId }, transaction: transaction);

                        transaction.Commit();
                        return result;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<EmployeeDTO> GetById(int id)
        {
            await using (var db = new SqlConnection(_connectionString))
            {
                string query = """                  
                    SELECT e.Id,
                    	   e.Name,
                    	   e.Surname,
                    	   e.Phone,
                           e.CompanyId,
                    	   p.Id as PassportId,
                    	   p.Type as PassportType,
                    	   p.Number as PassportNumber,
                    	   d.Id as DepartmentId,
                    	   d.Name as DepartmentName,
                    	   d.Phone as DepartmentPhone
                    FROM Employees e
                    INNER JOIN Passports p
                    	ON e.PassportId = p.Id
                    INNER JOIN Departments d
                    	ON e.DepartmentId = d.Id
                    WHERE e.Id=@Id;
                    """;

                var result = await db.QueryFirstOrDefaultAsync<Employee>(query, new { Id = id });

                if (result != null)
                {
                    return MapToDto(result);
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<IEnumerable<EmployeeDTO>> GetEmployeesByCompanyAsync(int companyId)
        {
            await using (var db = new SqlConnection(_connectionString))
            {
                string query = """
                    SELECT e.Id,
                    	   e.Name,
                    	   e.Surname,
                    	   e.Phone,
                           e.CompanyId,
                    	   p.Id as PassportId,
                    	   p.Type as PassportType,
                    	   p.Number as PassportNumber,
                    	   d.Id as DepartmentId,
                    	   d.Name as DepartmentName,
                    	   d.Phone as DepartmentPhone
                    FROM Employees e
                    INNER JOIN Passports p
                    	ON e.PassportId = p.Id
                    INNER JOIN Departments d
                    	ON e.DepartmentId = d.Id
                    INNER JOIN Companies c
                    	ON e.CompanyId = c.Id
                    WHERE c.Id = @CompanyId
                    """;
                var result = await db.QueryAsync<Employee>(query, new { companyId });
                return result.Select(MapToDto);
            }
        }

        public async Task<IEnumerable<EmployeeDTO>> GetEmployeesByDepartmentAsync(int companyId, int departmentId)
        {
            await using (var db = new SqlConnection(_connectionString))
            {
                string query = """
                    SELECT e.Id,
                    	   e.Name,
                    	   e.Surname,
                    	   e.Phone,
                           e.CompanyId,
                    	   p.Id as PassportId,
                    	   p.Type as PassportType,
                    	   p.Number as PassportNumber,
                    	   d.Id as DepartmentId,
                    	   d.Name as DepartmentName,
                    	   d.Phone as DepartmentPhone
                    FROM Employees e
                    INNER JOIN Passports p
                    	ON e.PassportId = p.Id
                    INNER JOIN Departments d
                    	ON e.DepartmentId = d.Id
                    INNER JOIN Companies c
                    	ON e.CompanyId = c.Id
                    WHERE c.Id = @companyId
                    AND d.Id = @departmentId;
                    """;
                var result = await db.QueryAsync<Employee>(query, new { companyId, departmentId });
                return result.Select(MapToDto);
            }
        }

        private async Task<bool> DepartmentExists(int departmentId)
        {
            await using (var db = new SqlConnection(_connectionString))
            {
                var departmentQuery = """
                        SELECT 1 
                        FROM Departments
                        WHERE Id = @Id
                        """;
                var departmentExists = await db.QueryFirstOrDefaultAsync<bool>(departmentQuery, new { Id = departmentId });
                return departmentExists;
            }
        }

        private EmployeeDTO MapToDto(Employee employee)
            => new EmployeeDTO()
            {
                Id = employee.Id,
                Name = employee.Name,
                Surname = employee.Surname,
                Phone = employee.Phone,
                CompanyId = employee.CompanyId,
                Passport = new Passport()
                {
                    Id = employee.PassportId,
                    Type = employee.PassportType,
                    Number = employee.PassportNumber
                },
                Department = new Department()
                {
                    Id = employee.DepartmentId,
                    Name = employee.DepartmentName,
                    Phone = employee.DepartmentPhone
                }
            };
    }
}
