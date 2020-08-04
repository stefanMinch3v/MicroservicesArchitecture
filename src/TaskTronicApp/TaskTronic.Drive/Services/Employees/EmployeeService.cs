namespace TaskTronic.Drive.Services.Employees
{
    using AutoMapper;
    using Data;
    using Data.DapperRepo;
    using Data.Models;
    using Microsoft.EntityFrameworkCore;
    using Models.Employees;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using TaskTronic.Services;

    public class EmployeeService : DataService<Employee>, IEmployeeService
    {
        private readonly IMapper mapper;
        private readonly IFileDAL fileDAL;
        private readonly IFolderDAL folderDAL;

        public EmployeeService(
            DriveDbContext db, 
            IMapper mapper,
            IFileDAL fileDAL,
            IFolderDAL folderDAL)
            : base(db)
        {
            this.mapper = mapper;
            this.fileDAL = fileDAL;
            this.folderDAL = folderDAL;
        }

        public async Task<int> GetIdByUserAsync(string userId)
            => await this.FindByUserAsync(userId, employee => employee.EmployeeId);

        public async Task<string> GetEmailByIdAsync(int employeeId)
            => await this.All()
                .Where(e => e.EmployeeId == employeeId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();

        public async Task SaveAsync(string userId, string email, string name)
        {
            var existing = await this.All().FirstOrDefaultAsync(e => e.Email == email);

            if (!(existing is null) || string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            await this.Save(new Employee { Email = email, UserId = userId, Name = name });
        }

        public async Task<IReadOnlyCollection<OutputEmployeeDetailsServiceModel>> GetAllAsync()
        {
            var employees = await this.mapper
                .ProjectTo<OutputEmployeeDetailsServiceModel>(this.All())
                .ToListAsync();

            foreach (var employee in employees)
            {
                employee.TotalFiles = await this.fileDAL.CountFilesForEmployeeAsync(employee.Id);

                employee.TotalFolders = await this.folderDAL.CountFoldersForEmployeeAsync(employee.Id);
            }

            return employees;
        }

        public async Task<Employee> FindByIdAsync(int employeeId)
            => await this.Data.FindAsync<Employee>(employeeId);

        public async Task<Employee> FindByUserAsync(string userId)
            => await this.FindByUserAsync(userId, employee => employee);

        public async Task<OutputEmployeeDetailsServiceModel> GetDetails(int employeeId)
            => await this.mapper
                .ProjectTo<OutputEmployeeDetailsServiceModel>(this.All()
                    .Where(d => d.EmployeeId == employeeId))
                .FirstOrDefaultAsync();

        public async Task<string> GetUserIdByEmployeeAsync(int employeeId)
            => await base.All()
                .Where(e => e.EmployeeId == employeeId)
                .Select(e => e.UserId)
                .FirstOrDefaultAsync();

        public async Task<int> GetCompanyDepartmentsIdAsync(string userId)
            => (await this.FindByUserAsync(userId)).CompanyDepartmentsId;

        public async Task<int> GetSelectedCompanyDepartmentId(int employeeId)
            => await this.All()
                .Where(e => e.EmployeeId == employeeId)
                .Select(e => e.CompanyDepartmentsId)
                .FirstOrDefaultAsync();
                

        public async Task SetCompanyDepartmentsIdAsync(string userId, int companyId, int departmentId)
        {
            var employee = await this.FindByUserAsync(userId);
            
            // TODO: only admin should be able to switch the employee company/department
            if (employee is null)
            {
                return;
            }

            var companyDepartment = await base.Data.Set<CompanyDepartments>()
                .FirstOrDefaultAsync(cd => cd.CompanyId == companyId 
                    && cd.DepartmentId == departmentId);

            if (companyDepartment is null)
            {
                return;
            }

            employee.CompanyDepartmentsId = companyDepartment.Id;

            await base.Data.SaveChangesAsync();
        }

        private async Task<T> FindByUserAsync<T>(
            string userId,
            Expression<Func<Employee, T>> selectorExpression)
        {
            var employeeInfo = await this.All()
                .Where(u => u.UserId == userId)
                .Select(selectorExpression)
                .FirstOrDefaultAsync();

            if (employeeInfo is null)
            {
                throw new InvalidOperationException("This user is not a valid employee.");
            }

            return employeeInfo;
        }
    }
}
