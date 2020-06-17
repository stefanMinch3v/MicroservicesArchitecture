namespace TaskTronic.Drive.Controllers
{
    using Common;
    using Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Services.Files;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using TaskTronic.Controllers;
    using TaskTronic.Drive.Services.Employees;
    using TaskTronic.Services.Identity;

    public class FilesController : ApiController
    {
        private const int MAX_ALLOWED_FILE_SIZE = int.MaxValue; // The data column cannot hold more than 2^31-1 bytes (~2GB)
        private readonly IFileService fileService;
        private readonly IEmployeeService employeeService;
        private readonly ICurrentUserService currentUser;

        public FilesController(
            IFileService fileService, 
            IEmployeeService employeeService,
            ICurrentUserService currentUser)
        {
            this.fileService = fileService;
            this.employeeService = employeeService;
            this.currentUser = currentUser;
        }

        [Route(nameof(GetFileById)), HttpGet]
        public async Task<ActionResult<FileServiceModel>> GetFileById(int catId, int folderId, int fileId)
            => await this.fileService.GetFileByIdAsync(catId, folderId, fileId);

        [Route(nameof(DownloadFile)), HttpGet]
        public async Task<ActionResult> DownloadFile(int catId, int folderId, int fileId, bool shouldOpen = false)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest();
            }

            var downloadFile = await this.fileService.GetFileInfoForDownloadAsync(
                catId,
                employeeId, 
                folderId, 
                fileId);

            if (downloadFile is null)
            {
                return NotFound();
            }

            var dispositionAttribute = shouldOpen ? "inline" : "attachment";

            Response.Headers.Add("Content-Disposition", $"{dispositionAttribute}; filename={downloadFile.FileName}");
            Response.Headers.Add("Content-Length", $"{downloadFile.FileSize}");
            Response.Headers.Add("Content-Type", $"{downloadFile.ContentType}");

            await this.fileService.ReadStreamFromFileAsync(downloadFile.BlobId, Response.Body);

            return new EmptyResult();
        }

        [Route(nameof(MoveFile)), HttpPost]
        public async Task<ActionResult<bool>> MoveFile(int fileId, int catId, int folderId, int newFolderId)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest();
            }

            return await this.fileService.MoveFileAsync(catId, folderId, fileId, newFolderId, employeeId);
        }

        [Route(nameof(RenameFile)), HttpPost]
        public async Task<ActionResult<bool>> RenameFile(int catId, int folderId, int fileId, string name)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest();
            }

            return await this.fileService.RenameFileAsync(catId, folderId, fileId, employeeId, name);
        }

        [Route(nameof(DeleteFile)), HttpDelete]
        public async Task<ActionResult<bool>> DeleteFile(int catId, int folderId, int fileId)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest();
            }

            return await this.fileService.DeleteFileAsync(employeeId, catId, folderId, fileId);
        }

        [Route(nameof(UploadFileToFolder)), HttpPost]
        public async Task<ActionResult<bool>> UploadFileToFolder(IFormFile file)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest();
            }

            if (file.Length > MAX_ALLOWED_FILE_SIZE)
            {
                throw new FileException { Message = $"The file is too large. {file.Length} exceeds the maximum size of {MAX_ALLOWED_FILE_SIZE}." };
            }

            var form = Request.Form;

            var extension = Path.GetExtension(form["name"]);

            var contentType = GetMimeType(extension);

            bool.TryParse(form["replaceExistingFiles"], out bool replaceExistingFiles);

            var model = this.GenerateInputFileModel(
                int.Parse(form["catId"]),
                int.Parse(form["folderId"]),
                contentType,
                extension,
                Path.GetFileNameWithoutExtension(form["name"]),
                employeeId,
                int.Parse(form["chunk"]),
                int.Parse(form["chunks"]),
                replaceExistingFiles,
                file.Length,
                file.OpenReadStream());

            return await this.fileService.UploadFileAsync(model);
        }

        [Route(nameof(SearchForFiles)), HttpGet]
        public async Task<ActionResult<IReadOnlyCollection<OutputFileServiceModel>>> SearchForFiles(int catalogId, string value)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest();
            }

            return (await this.fileService.SearchFilesAsync(catalogId, employeeId, value)).ToArray();
        }

        [Route(nameof(CreateNewFile)), HttpPost]
        public async Task<ActionResult<bool>> CreateNewFile(int catalogId, int folderId, NewFileType newFileType)
        {
            var employeeId = await this.employeeService.GetIdByUserAsync(this.currentUser.UserId);

            if (employeeId == 0)
            {
                return BadRequest();
            }

            return await this.fileService.CreateNewFileAsync(catalogId, employeeId, folderId, newFileType);
        }

        private static string GetMimeType(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (!extension.StartsWith(".", StringComparison.Ordinal))
            {
                extension = "." + extension;
            }

            return MimeTypes.mappings.TryGetValue(extension, out string mime) ? mime : "application/octet-stream";
        }

        private InputFileServiceModel GenerateInputFileModel(
            int catId,
            int folderId,
            string contentType,
            string fileType,
            string name,
            int employeeId,
            int chunk,
            int chunks,
            bool replaceExistingFiles,
            long length,
            Stream openReadStream)
            => new InputFileServiceModel
            {
                CatalogId = catId,
                FolderId = folderId,
                ContentType = contentType,
                FileType = fileType,
                FileName = name,
                EmployeeId = employeeId,
                Chunk = chunk,
                Chunks = chunks,
                ReplaceExistingFiles = replaceExistingFiles,
                Filesize = length,
                Stream = openReadStream
            };
    }
}
