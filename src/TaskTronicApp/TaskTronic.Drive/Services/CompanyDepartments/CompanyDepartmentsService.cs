namespace TaskTronic.Drive.Services.CompanyDepartments
{
    using AutoMapper;
    using Data.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TaskTronic.Common;
    using TaskTronic.Drive.Data;
    using TaskTronic.Drive.Exceptions;
    using TaskTronic.Services;

    public class CompanyDepartmentsService : DataService<Company>, ICompanyDepartmentsService
    {
        private readonly IMapper mapper;

        public CompanyDepartmentsService(
            DriveDbContext dbContext,
            IMapper mapper)
            : base(dbContext) => this.mapper = mapper;

        public async Task<bool> AddDepartmentToCompany(int companyId, int departmentId)
        {
            var existingCompanyDepartment = await this.Data.Set<CompanyDepartments>()
                .FirstOrDefaultAsync(cd => cd.CompanyId == companyId && cd.DepartmentId == departmentId);

            if (existingCompanyDepartment != null)
            {
                return false;
            }

            var companyDepartment = new CompanyDepartments
            {
                CompanyId = companyId,
                DepartmentId = departmentId
            };

            this.Data.Set<CompanyDepartments>()
                .Add(companyDepartment);

            await this.Data.SaveChangesAsync();

            return true;
        }

        public async Task Create(string name)
        {
            Guard.AgainstEmptyString<CompanyDepartmentException>(name);

            var model = new Company { Name = name };

            this.Data.Add(model);

            await this.Data.SaveChangesAsync();
        }

        public async Task CreateDepartment(string name)
        {
            Guard.AgainstEmptyString<CompanyDepartmentException>(name);

            var model = new Department { Name = name };

            this.Data.Add(model);

            await this.Data.SaveChangesAsync();
        }

        public async Task<OutputCompaniesServiceModel> GetAllAsync(string userId)
        {
            var outputCompanies = new OutputCompaniesServiceModel
            {
                Companies = await this.mapper
                    .ProjectTo<OutputCompanyDepartmentsServiceModel>(this.All())
                    .ToListAsync()
            };

            var employee = await this.Data.Set<Employee>()
                .Include(e => e.CompanyDepartments)
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (employee != null && employee.CompanyDepartments != null)
            {
                var selectedCompanyId = employee.CompanyDepartments.CompanyId;
                var selectedDepartmentId = employee.CompanyDepartments.DepartmentId;

                if (selectedCompanyId != 0)
                {
                    outputCompanies.SelectedData = new OutputSelectedCompanyServiceModel
                    {
                        CompanyId = selectedCompanyId,
                        DepartmentId = selectedDepartmentId
                    };
                }
            }

            return outputCompanies;
        }

        public async Task<OutputCompaniesAndDepartmentsServiceModel> GetAllAsync()
            => new OutputCompaniesAndDepartmentsServiceModel
            {
                Companies = await this.mapper
                    .ProjectTo<OutputCompanyDepartmentsServiceModel>(this.All())
                    .ToListAsync(),
                Departments = await this.mapper
                    .ProjectTo<OutputDepartmentServiceModel>(this.Data.Set<Department>())
                    .ToListAsync()
            };

        public async Task<IReadOnlyList<OutputDepartmentServiceModel>> AllNotInCompany(int companyId)
            => await this.mapper
                .ProjectTo<OutputDepartmentServiceModel>(this.Data.Set<Department>()
                    .Where(d => !d.Companies.Any(c => c.CompanyId == companyId)))
                .ToListAsync();
    }
}
