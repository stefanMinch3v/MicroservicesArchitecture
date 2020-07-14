namespace TaskTronic.Drive.Data.DapperRepo
{
    using System.Threading.Tasks;

    public interface ICatalogDAL
    {
        Task<int?> GetAsync(int companyDepartmentsId);

        Task<int> AddAsync(int companyDepartmentsId);
    }
}
