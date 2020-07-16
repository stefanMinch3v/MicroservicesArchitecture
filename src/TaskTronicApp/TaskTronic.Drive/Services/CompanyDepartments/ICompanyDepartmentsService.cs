namespace TaskTronic.Drive.Services.CompanyDepartments
{
    using System.Threading.Tasks;

    public interface ICompanyDepartmentsService
    {
        Task<OutputCompaniesServiceModel> GetAllAsync(string userId);
    }
}
