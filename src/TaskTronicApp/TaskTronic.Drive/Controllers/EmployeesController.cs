namespace TaskTronic.Drive.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TaskTronic.Controllers;
    using TaskTronic.Drive.Services.CompanyDepartments;
    using TaskTronic.Drive.Services.Employees;
    using TaskTronic.Infrastructure;
    using TaskTronic.Services;
    using TaskTronic.Services.Identity;

    [Authorize]
    public class EmployeesController : ApiController
    {
        private readonly ICurrentUserService currentUser;
        private readonly IEmployeeService employeeService;
        private readonly ICompanyDepartmentsService companyDepartments;

        public EmployeesController(
            ICurrentUserService currentUser,
            IEmployeeService employeeService,
            ICompanyDepartmentsService companyDepartments)
        {
            this.currentUser = currentUser;
            this.employeeService = employeeService;
            this.companyDepartments = companyDepartments;
        }

        [HttpGet]
        [Route(nameof(GetCompanyDepartmentSignId))]
        public async Task<ActionResult<int>> GetCompanyDepartmentSignId()
        {
            if (this.currentUser.IsAdministrator)
            {
                return BadRequest("Must be an employee.");
            }

            return Ok(await this.employeeService.GetCompanyDepartmentsIdAsync(this.currentUser.UserId));
        }

        [HttpGet]
        [Route(nameof(GetCompanyDepartments))]
        public async Task<ActionResult<OutputCompaniesServiceModel>> GetCompanyDepartments()
            => await this.companyDepartments.GetAllAsync(this.currentUser.UserId);

        [HttpPost]
        [Route(nameof(SetCompanyDepartmentSignId))]
        public async Task<ActionResult> SetCompanyDepartmentSignId(int companyId, int departmentId)
        {
            if (this.currentUser.IsAdministrator)
            {
                return BadRequest("Must be an employee.");
            }

            await this.employeeService.SetCompanyDepartmentsIdAsync(
                this.currentUser.UserId,
                companyId,
                departmentId);

            return Ok();
        }

        [HttpPut]
        [Route(Id)]
        public async Task<ActionResult> Edit(int id, EditEmployeeInputModel input)
        {
            var employee = this.currentUser.IsAdministrator
                ? await this.employeeService.FindByIdAsync(id)
                : await this.employeeService.FindByUserAsync(this.currentUser.UserId);

            if (id != employee.EmployeeId)
            {
                return BadRequest(Result.Failure(this.currentUser.UserId));
            }

            employee.Name = input.Name;
            employee.Email = input.Email;

            await this.employeeService.Save(employee);

            return Ok();
        }

        [HttpGet]
        [Route(Id)]
        public async Task<ActionResult<EmployeeDetailsOutputModel>> Details(int id)
            => await this.employeeService.GetDetails(id);

        [HttpGet]
        [AuthorizeAdministrator]
        public async Task<IReadOnlyCollection<EmployeeDetailsOutputModel>> All()
            => await this.employeeService.GetAllAsync();
    }
}
