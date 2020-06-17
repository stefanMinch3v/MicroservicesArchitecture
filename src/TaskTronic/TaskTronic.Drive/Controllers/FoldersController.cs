namespace TaskTronic.Drive.Controllers
{
    using Drive.Services.Catalogs;
    using Drive.Services.Files;
    using Drive.Services.Folders;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TaskTronic.Controllers;
    using TaskTronic.Services.Identity;

    public class FoldersController : ApiController
    {
        private readonly IFolderService folderService;
        private readonly ICatalogService catalogService;
        private readonly IFileService fileService;
        private readonly ICurrentUserService currentUser;

        public FoldersController(
            IFolderService folderService,
            ICatalogService catalogService,
            IFileService fileService,
            ICurrentUserService currentUser)
        {
            this.folderService = folderService;
            this.catalogService = catalogService;
            this.fileService = fileService;
            this.currentUser = currentUser;
        }

        [Route(nameof(CreateFolder)), HttpPost]
        public async Task<bool> CreateFolder(int catId, int rootId, int parentFolderId, string name, bool isPrivate = false)
            => await this.folderService.CreateFolderAsync(
                    new InputFolderServiceModel
                    {
                        CatalogId = catId,
                        Name = name,
                        ParentId = parentFolderId,
                        RootId = rootId,
                        IsPrivate = isPrivate,
                        UserId = this.currentUser.UserId
                    });

        [Route(nameof(RenameFolder)), HttpPost]
        public async Task<bool> RenameFolder(int catId, int folderId, string name)
            => await this.folderService.RenameFolderAsync(catId, folderId, this.currentUser.UserId, name);

        [Route(nameof(MoveFolder)), HttpPost]
        public async Task<bool> MoveFolder(int catId, int folderId, int newFolderId)
            => await this.folderService.MoveFolderAsync(catId, folderId, this.currentUser.UserId, newFolderId);

        [Route(nameof(GetRootFolder)), HttpGet]
        public async Task<FolderServiceModel> GetRootFolder(int companyId, int departmentId)
        {
            var catalogId = await this.catalogService.GetIdAsync(companyId, departmentId, this.currentUser.UserId);

            return await this.folderService.GetRootFolderByCatIdAsync(
                catalogId,
                this.currentUser.UserId);
        }

        [Route(nameof(GetFolderById)), HttpGet]
        public async Task<FolderServiceModel> GetFolderById(int folderId)
            => await this.folderService.GetFolderByIdAsync(folderId, this.currentUser.UserId);

        [Route(nameof(CheckFilesNamesForFolder)), HttpGet]
        public async Task<Dictionary<string, bool>> CheckFilesNamesForFolder(int catId, int folderId)
        {
            var fileNames = HttpContext.Request.Query["fileNames"];

            return await this.fileService.CheckFilesInFolderForCollisions(
                catId,
                this.currentUser.UserId,
                folderId,
                fileNames);
        }

        [Route(nameof(DeleteFolder)), HttpDelete]
        public async Task<bool> DeleteFolder(int catId, int folderId)
            => await this.folderService.DeleteFolderAsync(catId, this.currentUser.UserId, folderId);
    }
}
