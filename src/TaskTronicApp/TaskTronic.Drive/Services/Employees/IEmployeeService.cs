namespace TaskTronic.Drive.Services.Employees
{
    using Data.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TaskTronic.Services;

    public interface IEmployeeService : IDataService<Employee>
    {
        Task<int> GetIdByUserAsync(string userId);

        Task<string> GetEmailByIdAsync(int employeeId);

        Task SaveAsync(string userId, string email, string name);

        Task<IReadOnlyCollection<EmployeeDetailsOutputModel>> GetAllAsync();

        Task<EmployeeDetailsOutputModel> GetDetails(int employeeId);

        Task<Employee> FindByIdAsync(int employeeId);

        Task<Employee> FindByUserAsync(string userId);

        Task<string> GetUserIdByEmployeeAsync(int employeeId);

        Task<int> GetCompanyDepartmentsIdAsync(string userId);

        Task SetCompanyDepartmentsIdAsync(string userId, int companyId, int departmentId);
    }
}
