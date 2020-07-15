namespace TaskTronic.Drive.Services.CompanyDepartments
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ICompanyDepartmentsService
    {
        Task<IReadOnlyList<OutputCompanyDepartmentsServiceModel>> GetAllAsync();
    }
}
