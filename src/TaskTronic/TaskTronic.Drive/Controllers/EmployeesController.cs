namespace TaskTronic.Drive.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using TaskTronic.Controllers;
    using TaskTronic.Drive.Services.Employees;
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
    }
}
