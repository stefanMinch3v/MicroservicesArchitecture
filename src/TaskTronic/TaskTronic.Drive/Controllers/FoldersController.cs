namespace TaskTronic.Drive.Controllers
{
    using Drive.Services.Catalogs;
    using Drive.Services.Files;
    using Drive.Services.Folders;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TaskTronic.Controllers;
    using TaskTronic.Drive.Services.Employees;
    using TaskTronic.Services.Identity;

    public class FoldersController : ApiController
    {
        private readonly IFolderService folderService;
        private readonly ICatalogService catalogService;
        private readonly IFileService fileService;
        private readonly ICurrentUserService currentUser;
        private readonly IEmployeeService employeeService;

        public FoldersController(
            IFolderService folderService,
            ICatalogService catalogService,
            IFileService fileService,
            ICurrentUserService currentUser,
            IEmployeeService employeeService)
        {
            this.folderService = folderService;
            this.catalogService = catalogService;
            this.fileService = fileService;
            this.currentUser = currentUser;
            this.employeeService = employeeService;
        }

        [Route(nameof(CreateFolder)), HttpPost]
        public async Task<ActionResult<bool>> CreateFolder(
            int catId, 
            int rootId, 
            int parentFolderId, 
            string name, 
            bool isPrivate = false)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest();
            }

            return await this.folderService.CreateFolderAsync(
                new InputFolderServiceModel
                {
                    CatalogId = catId,
                    Name = name,
                    ParentId = parentFolderId,
                    RootId = rootId,
                    IsPrivate = isPrivate,
                    EmployeeId = employeeId
                });
        }

        [Route(nameof(RenameFolder)), HttpPost]
        public async Task<ActionResult<bool>> RenameFolder(int catId, int folderId, string name)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest();
            }

            return await this.folderService.RenameFolderAsync(catId, folderId, employeeId, name);
        }

        [Route(nameof(MoveFolder)), HttpPost]
        public async Task<ActionResult<bool>> MoveFolder(int catId, int folderId, int newFolderId)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest();
            }

            return await this.folderService.MoveFolderAsync(catId, folderId, employeeId, newFolderId);
        }

        [Route(nameof(GetRootFolder)), HttpGet]
        public async Task<ActionResult<FolderServiceModel>> GetRootFolder(int companyId, int departmentId)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest();
            }

            var catalogId = await this.catalogService.GetIdAsync(companyId, departmentId, employeeId);
            return await this.folderService.GetRootFolderByCatIdAsync(catalogId, employeeId);
        }

        [Route(nameof(GetFolderById)), HttpGet]
        public async Task<ActionResult<FolderServiceModel>> GetFolderById(int folderId)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest();
            }

            return await this.folderService.GetFolderByIdAsync(folderId, employeeId);
        }

        [Route(nameof(CheckFilesNamesForFolder)), HttpGet]
        public async Task<ActionResult<Dictionary<string, bool>>> CheckFilesNamesForFolder(int catId, int folderId)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest();
            }

            var fileNames = HttpContext.Request.Query["fileNames"];

            return await this.fileService.CheckFilesInFolderForCollisions(
                catId,
                employeeId,
                folderId,
                fileNames);
        }

        [Route(nameof(DeleteFolder)), HttpDelete]
        public async Task<ActionResult<bool>> DeleteFolder(int catId, int folderId)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest();
            }

            return await this.folderService.DeleteFolderAsync(catId, employeeId, folderId);
        }
    }
}
