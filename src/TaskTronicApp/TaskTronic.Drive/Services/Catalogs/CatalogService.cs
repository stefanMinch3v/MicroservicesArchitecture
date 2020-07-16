namespace TaskTronic.Drive.Services.Catalogs
{
    using Data.DapperRepo;
    using Folders;
    using System;
    using System.Threading.Tasks;

    public class CatalogService : ICatalogService
    {
        private readonly ICatalogDAL catalogDAL;
        private readonly IFolderService folderService;

        public CatalogService(
            ICatalogDAL catalogDAL,
            IFolderService folder)
        {
            this.catalogDAL = catalogDAL;
            this.folderService = folder;
        }

        public async Task<int> GetIdAsync(int companyDepartmentsId, int employeeId)
        {
            var catalogId = await this.catalogDAL.GetAsync(companyDepartmentsId);

            if (catalogId == 0)
            {
                var createdCatId = await this.catalogDAL.AddAsync(companyDepartmentsId);

                await this.folderService.CreateFolderAsync(this.CreateInputFolderModel(createdCatId, employeeId));

                return createdCatId;
            }

            return catalogId;
        }

        private InputFolderServiceModel CreateInputFolderModel(int catId, int employeeId)
            => new InputFolderServiceModel
            {
                CatalogId = catId,
                EmployeeId = employeeId,
                Name = DriveConstants.ROOT_FOLDER_NAME,
                CreateDate = DateTimeOffset.UtcNow
            };
    }
}
