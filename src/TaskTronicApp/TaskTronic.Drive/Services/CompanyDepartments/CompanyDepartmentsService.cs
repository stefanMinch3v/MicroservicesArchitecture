namespace TaskTronic.Drive.Services.CompanyDepartments
{
    using AutoMapper;
    using Data.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
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

        public async Task<IReadOnlyList<OutputCompanyDepartmentsServiceModel>> GetAllAsync()
            => await this.mapper
                .ProjectTo<OutputCompanyDepartmentsServiceModel>(this.All())
                .ToListAsync();
    }
}
