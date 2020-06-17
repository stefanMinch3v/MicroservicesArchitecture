namespace TaskTronic.Drive.Services.Catalogs
{
    using System.Threading.Tasks;

    public interface ICatalogService
    {
        Task<int> GetIdAsync(int companyId, int departmentId, int employeeId);
    }
}
