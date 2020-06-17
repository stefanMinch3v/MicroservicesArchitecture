namespace TaskTronic.Drive.Services.Employees
{
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using Microsoft.EntityFrameworkCore;

    public class EmployeeService : IEmployeeService
    {
        private readonly DriveDbContext db;

        public EmployeeService(DriveDbContext db)
            => this.db = db;

        public async Task<int> GetIdByUserAsync(string userId)
            => await this.db.Employees
                .Where(e => e.UserId == userId)
                .Select(u => u.EmployeeId)
                .FirstOrDefaultAsync();

        public async Task<string> GetEmailByIdAsync(int employeeId)
            => await this.db.Employees
                .Where(e => e.EmployeeId == employeeId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();
    }
}
