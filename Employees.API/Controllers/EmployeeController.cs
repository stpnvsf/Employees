using Microsoft.AspNetCore.Mvc;
using Employees.Application;

namespace Employees.API.Controllers
{
    [Route("employees")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IRepository _employeeRepository;

        public EmployeeController(IRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDTO>> GetById(int id)
        {
            try
            {
                var result = await _employeeRepository.GetById(id);
                if (result == null)
                {
                    return NotFound(id);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEmployee(int id)
        {
            try
            {
                var item = await _employeeRepository.GetById(id);
                if (item == null)
                {
                    return NotFound(id);
                }

                var result = await _employeeRepository.DeleteEmployee(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetByCompany/{companyId}")]
        public async Task<ActionResult<IEnumerable<EmployeeDTO>>> GetByCompany(int companyId)
        {
            try
            {


                var items = await _employeeRepository.GetEmployeesByCompanyAsync(companyId);
                if (items == null)
                {
                    return NotFound();
                }

                return Ok(items);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetByCompany/{companyId}/ByDepartment/{departmentId}")]
        public async Task<ActionResult<IEnumerable<EmployeeDTO>>> GetByDepartment(int companyId, int departmentId)
        {
            try
            {
                var items = await _employeeRepository.GetEmployeesByDepartmentAsync(companyId, departmentId);
                if (items == null)
                {
                    return NotFound();
                }

                return Ok(items);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult> ChangeEmployee(int id, UpdateEmployeeDTO updateDto)
        {
            try
            {
                var currentEmployee = await _employeeRepository.GetById(updateDto.Id);
                Ok(currentEmployee);
                if (currentEmployee == null)
                {
                    return NotFound($"Employee with id = {updateDto.Id} not found");
                }

                MapUpdatedFields(updateDto, currentEmployee);
                var res = await _employeeRepository.ChangeEmployeeAsync(currentEmployee);

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateEmployee(CreateEmployeeDTO dto)
        {
            try
            {
                var res = await _employeeRepository.AddEmployeeAsync(dto);
                return Ok(res);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        private void MapUpdatedFields(UpdateEmployeeDTO updateDto, EmployeeDTO currentEmployee)
        {

            if (updateDto.Name != null)
            {
                currentEmployee.Name = updateDto.Name;
            }
            if (updateDto.Surname != null)
            {
                currentEmployee.Surname = updateDto.Surname;
            }
            if (updateDto.Phone != null)
            {
                currentEmployee.Phone = updateDto.Phone;
            }
            if (updateDto.CompanyId != null)
            {
                currentEmployee.CompanyId = updateDto.CompanyId.Value;
            }
            if (updateDto.DepartmentId != null)
            {
                currentEmployee.Department.Id = updateDto.DepartmentId.Value;
            }
            if (updateDto.Passport != null)
            {
                var updatePassport = new UpdatePassport();

                if (updateDto.Passport.Number != null)
                {
                    updatePassport.Number = updateDto.Passport.Number;
                }

                if (updateDto.Passport.Type != null)
                {
                    updatePassport.Type = updateDto.Passport.Type;
                }

                updateDto.Passport = updatePassport;
            }
        }
    }
}
