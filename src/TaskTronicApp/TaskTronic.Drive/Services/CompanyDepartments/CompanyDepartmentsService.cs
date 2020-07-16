namespace TaskTronic.Drive.Services.CompanyDepartments
{
    using AutoMapper;
    using Data.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;
    using TaskTronic.Drive.Data;
    using TaskTronic.Services;

    public class CompanyDepartmentsService : DataService<Company>, ICompanyDepartmentsService
    {
        private readonly IMapper mapper;

        public CompanyDepartmentsService(
            DriveDbContext dbContext,
            IMapper mapper)
            : base(dbContext) => this.mapper = mapper;

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
    }
}
