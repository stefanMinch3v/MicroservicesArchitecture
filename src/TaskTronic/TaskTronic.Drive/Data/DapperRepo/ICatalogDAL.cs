namespace TaskTronic.Drive.Data.DapperRepo
{
    using System.Threading.Tasks;
    using Services.Catalogs;

    public interface ICatalogDAL
    {
        Task<int?> GetAsync(int companyId, int departmentId);

        Task<int> AddAsync(int companyId, int departmentId);
    }
}
