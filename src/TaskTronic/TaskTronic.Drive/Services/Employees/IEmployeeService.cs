namespace TaskTronic.Drive.Services.Employees
{
    using System.Threading.Tasks;

    public interface IEmployeeService
    {
        Task<int> GetIdByUserAsync(string userId);
        Task<string> GetEmailByIdAsync(int employeeId);
        Task SaveAsync(string userId, string email);
    }
}
