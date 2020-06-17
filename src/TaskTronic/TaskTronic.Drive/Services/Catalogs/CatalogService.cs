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

        public async Task<int> GetIdAsync(int companyId, int departmentId, string userId)
        {
            var catalogId = await this.catalogDAL.GetAsync(companyId, departmentId);

            if (catalogId is null)
            {
                var createdCatId = await this.catalogDAL.AddAsync(companyId, departmentId);

                await this.folderService.CreateFolderAsync(this.CreateInputFolderModel(createdCatId, userId));

                return createdCatId;
            }

            return catalogId.Value;
        }

        private InputFolderServiceModel CreateInputFolderModel(int catId, string userId)
            => new InputFolderServiceModel
            {
                CatalogId = catId,
                UserId = userId,
                Name = "Root",
                CreateDate = DateTimeOffset.UtcNow
            };
    }
}
