namespace TaskTronic.Drive.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Services.Catalogs;
    using Services.Employees;
    using Services.Files;
    using Services.Folders;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TaskTronic.Controllers;
    using TaskTronic.Services.Identity;

    [Authorize]
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

        [HttpPost]
        [Route(nameof(CreateFolder))]
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
                return BadRequest(DriveConstants.INVALID_EMPLOYEE);
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

        [HttpPost]
        [Route(nameof(RenameFolder))]
        public async Task<ActionResult<bool>> RenameFolder(int catId, int folderId, string name)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest(DriveConstants.INVALID_EMPLOYEE);
            }

            return await this.folderService.RenameFolderAsync(catId, folderId, employeeId, name);
        }

        [HttpPost]
        [Route(nameof(MoveFolder))]
        public async Task<ActionResult<bool>> MoveFolder(int catId, int folderId, int newFolderId)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest(DriveConstants.INVALID_EMPLOYEE);
            }

            return await this.folderService.MoveFolderAsync(catId, folderId, employeeId, newFolderId);
        }

        [HttpGet]
        [Route(nameof(GetRootFolder))]
        public async Task<ActionResult<FolderServiceModel>> GetRootFolder(int companyId, int departmentId)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest(DriveConstants.INVALID_EMPLOYEE);
            }

            var catalogId = await this.catalogService.GetIdAsync(companyId, departmentId, employeeId);
            return await this.folderService.GetRootFolderByCatIdAsync(catalogId, employeeId);
        }

        [HttpGet]
        [Route(nameof(GetFolderById))]
        public async Task<ActionResult<FolderServiceModel>> GetFolderById(int folderId)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest(DriveConstants.INVALID_EMPLOYEE);
            }

            return await this.folderService.GetFolderByIdAsync(folderId, employeeId);
        }

        [HttpGet]
        [Route(nameof(CheckFilesNamesForFolder))]
        public async Task<ActionResult<Dictionary<string, bool>>> CheckFilesNamesForFolder(int catId, int folderId)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest(DriveConstants.INVALID_EMPLOYEE);
            }

            var fileNames = HttpContext.Request.Query["fileNames"];

            return await this.fileService.CheckFilesInFolderForCollisions(
                catId,
                employeeId,
                folderId,
                fileNames);
        }

        [HttpDelete]
        [Route(nameof(DeleteFolder))]
        public async Task<ActionResult<bool>> DeleteFolder(int catId, int folderId)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest(DriveConstants.INVALID_EMPLOYEE);
            }

            return await this.folderService.DeleteFolderAsync(catId, employeeId, folderId);
        }

        [HttpGet]
        [Route(nameof(GetMine))]
        public async Task<ActionResult<IReadOnlyCollection<OutputFolderFlatServiceModel>>> GetMine()
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest(DriveConstants.INVALID_EMPLOYEE);
            }

            return Ok(await this.folderService.GetAllForEmployeeAsync(employeeId));
        }
    }
}
