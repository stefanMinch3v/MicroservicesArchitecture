namespace TaskTronic.Drive.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TaskTronic.Controllers;
    using TaskTronic.Drive.Services.Employees;
    using TaskTronic.Infrastructure;
    using TaskTronic.Services;
    using TaskTronic.Services.Identity;

    [Authorize]
    public class EmployeesController : ApiController
    {
        private readonly ICurrentUserService currentUser;
        private readonly IEmployeeService employeeService;

        public EmployeesController(
            ICurrentUserService currentUser,
            IEmployeeService employeeService)
        {
            this.currentUser = currentUser;
            this.employeeService = employeeService;
        }

        [HttpPost]
        public async Task<ActionResult> Create(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(nameof(email));
            }

            var userId = this.currentUser.UserId;

            await this.employeeService.SaveAsync(userId, email);

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
