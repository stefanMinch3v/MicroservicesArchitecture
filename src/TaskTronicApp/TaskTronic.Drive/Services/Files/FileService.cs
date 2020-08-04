namespace TaskTronic.Drive.Services.Files
{
    using Data.DapperRepo;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;
    using Exceptions;
    using MassTransit;
    using Models.Files;
    using Models.Folders;
    using Services.Employees;
    using Services.Folders;
    using Services.Messages;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using TaskTronic.Common;
    using TaskTronic.Messages.Drive.Files;

    public class FileService : IFileService
    {
        private readonly IFileDAL fileDAL;
        private readonly IPermissionsDAL permissionsDAL;
        private readonly IFolderService folderService;
        private readonly IEmployeeService employeeService;
        private readonly IMessageService messageService;
        private readonly IBus publisher;

        public FileService(
            IFileDAL fileDAL,
            IPermissionsDAL permissionsDAL,
            IFolderService folder,
            IEmployeeService employeeService,
            IMessageService messageService,
            IBus publisher)
        {
            this.fileDAL = fileDAL;
            this.permissionsDAL = permissionsDAL;
            this.folderService = folder;
            this.employeeService = employeeService;
            this.messageService = messageService;
            this.publisher = publisher;
        }

        public async Task<IReadOnlyCollection<FileServiceModel>> GetFilesByFolderIdAsync(
            int catalogId, 
            int folderId, 
            int employeeId)
        {
            var isPrivate = await this.folderService.IsFolderPrivateAsync(folderId);

            if (isPrivate)
            {
                var hasPermission = await this.permissionsDAL.HasUserPermissionForFolderAsync(catalogId, folderId, employeeId);
                if (!hasPermission)
                {
                    throw new PermissionException { Message = "You do not have access to this folder." };
                }
            }

            return (await this.fileDAL.GetFilesByFolderIdAsync(folderId)).ToList();
        }

        public async Task<bool> DeleteFileAsync(int employeeId, int catalogId, int folderId, int fileId)
        {
            var isPrivate = await this.folderService.IsFolderPrivateAsync(folderId);
            if (isPrivate)
            {
                var hasPermission = await this.permissionsDAL.HasUserPermissionForFolderAsync(catalogId, folderId, employeeId);
                if (!hasPermission)
                {
                    throw new PermissionException { Message = "You do not have access to this folder." };
                }
            }

            var file = await this.fileDAL.GetFileByIdAsync(catalogId, folderId, fileId);
            if (file is null)
            {
                throw new FileException { Message = "File not found." };
            }

            var (success, insertedMessageId) = await this.fileDAL.DeleteFileAsync(
                file.CatalogId, 
                file.FolderId, 
                file.FileId, 
                file.BlobId);

            if (success)
            {
                // TODO: refactor when move to EF
                var messageData = new FileDeletedMessage
                {
                    FileId = fileId
                };

                await this.publisher.Publish(messageData);

                await this.messageService.MarkMessageAsPublishedAsync(insertedMessageId);
            }

            return success;
        }

        public async Task<bool> RenameFileAsync(int catalogId, int folderId, int fileId, int employeeId, string newFileName)
        {
            Guard.AgainstEmptyString<FileException>(newFileName, nameof(newFileName));
            Guard.AgainstInvalidWindowsCharacters<FileException>(newFileName, nameof(newFileName));

            var file = await this.fileDAL.GetFileByIdAsync(catalogId, folderId, fileId);
            if (file is null)
            {
                throw new FileException { Message = "File not found." };
            }

            await this.folderService.CheckFolderPermissionsAsync(catalogId, folderId, employeeId);

            return await this.fileDAL.RenameFileAsync(catalogId, folderId, fileId, newFileName);
        }

        public async Task<bool> UploadFileAsync(InputFileServiceModel file)
        {
            Guard.AgainstNullObject<FileException>(file, nameof(file));
            Guard.AgainstEmptyString<FileException>(file.FileName, nameof(file.FileName));
            Guard.AgainstInvalidWindowsCharacters<FileException>(file.FileName, nameof(file.FileName));

            var firstChunk = file.Chunk == 0;
            var lastChunk = file.Chunk == file.Chunks - 1;

            // first chunk 
            if (firstChunk)
            {
                // create blob row
                var blobCreated = await this.fileDAL.CreateBlobAsync(file);

                if (file.Chunks > 1)
                {
                    return blobCreated;
                }
            }

            // append til get to the last chunk (middle)
            if (!firstChunk && !lastChunk)
            {
                return await this.fileDAL.AppendChunkToBlobAsync(file);
            }

            // if last chunk append and save file
            if (!firstChunk)
            {
                await this.fileDAL.AppendChunkToBlobAsync(file);
            }

            var insertedFileId = 0;
            var insertedMessageId = 0;

            var existingFileId = await this.fileDAL.DoesFileWithSameNameExistInFolder(
                file.CatalogId, 
                file.FolderId, 
                file.FileName, 
                file.FileType);

            if (existingFileId.HasValue && existingFileId.Value > 0)
            {
                // file with name does exist in folder
                if (file.ReplaceExistingFiles)
                {
                    // TODO: Not yet added in the front-end as functionallity
                    // replace the existing file
                    //insertedId = await this.fileDAL.SaveCompletedUploadAsReplaceExistingFileAsync(file, existingFileId.Value);
                    throw new NotImplementedException("Add front end functionallity for it!");
                }
                else
                {
                    // add as new file with new fileName
                    // check if we need to change filename
                    var filesInFolder = await this.GetFilesByFolderIdAsync(file.CatalogId, file.FolderId, file.EmployeeId);

                    if (filesInFolder != null && filesInFolder.Any(f => f.FileName.Equals(file.FileName) && f.FileType.Equals(file.FileType)))
                    {
                        var oldFileName = file.FileName;
                        this.RenameFileName(filesInFolder, inputFileModel: file);

                        file.CreateDate = DateTimeOffset.UtcNow;
                        file.UpdateDate = DateTimeOffset.UtcNow;

                        // TODO: refactor to EF and move messages out
                        var insertedData = await this.fileDAL.SaveCompletedUploadAsync(file, oldFileName);
                        insertedFileId = insertedData.FileId;
                        insertedMessageId = insertedData.MessageId;
                    }
                }

                if (insertedFileId < 1)
                {
                    return false;
                }
            }
            else // file with name does NOT exist in folder
            {
                file.CreateDate = DateTimeOffset.UtcNow;
                file.UpdateDate = DateTimeOffset.UtcNow;

                // TODO: refactor to EF and move messages out
                var insertedData = await this.fileDAL.SaveCompletedUploadAsync(file);
                insertedFileId = insertedData.FileId;
                insertedMessageId = insertedData.MessageId;

                if (insertedFileId < 1)
                {
                    return false;
                }
            }

            if (insertedFileId > 0)
            {
                // TODO: refactor when move to EF
                var messageData = new FileUploadedMessage
                {
                    FileId = insertedFileId,
                    Name = file.FileName,
                    Type = file.ContentType
                };

                await this.publisher.Publish(messageData);

                await this.messageService.MarkMessageAsPublishedAsync(insertedMessageId);
            }

            return true;
        }

        public Task ReadStreamFromFileAsync(int blobId, Stream stream)
            => this.fileDAL.ReadStreamFromFileAsync(blobId, stream);

        public async Task<OutputFileDownloadServiceModel> GetFileInfoForDownloadAsync(
            int catalogId, 
            int employeeId, 
            int folderId, 
            int fileId)
        {
            var isPrivate = await this.folderService.IsFolderPrivateAsync(folderId);

            if (isPrivate)
            {
                var hasPermission = await this.permissionsDAL.HasUserPermissionForFolderAsync(catalogId, folderId, employeeId);
                if (!hasPermission)
                {
                    throw new PermissionException { Message = "The folder is private." };
                }
            }

            return await this.fileDAL.GetFileInfoForDownloadAsync(fileId);
        }

        public async Task<FileServiceModel> GetFileByIdAsync(int catalogId, int folderId, int fileId)
        {
            var file = await this.fileDAL.GetFileByIdAsync(catalogId, folderId, fileId);

            if (file is null)
            {
                throw new FileException { Message = "File not found." };
            }

            file.UpdaterUsername = await this.employeeService.GetEmailByIdAsync(file.EmployeeId);

            return file;
        }

        public async Task<bool> CreateNewFileAsync(int catalogId, int employeeId, int folderId, NewFileType fileType)
            => fileType switch
            {
                NewFileType.Word => await this.CreateEmptyWordDocAsync(catalogId, employeeId, folderId),
                NewFileType.Excel => await this.CreateEmptyExcelDocAsync(catalogId, employeeId, folderId),
                _ => throw new InvalidOperationException($"Unsupported file type: {fileType}"),
            };

        private void RenameFileName(IEnumerable<FileServiceModel> filesInFolder, InputFileServiceModel inputFileModel = null, FileServiceModel fileModel = null)
        {
            if (inputFileModel is null && fileModel is null)
            {
                throw new ArgumentNullException("File model cant be null.");
            }

            IFileContract file;

            if (inputFileModel is null)
            {
                file = fileModel;
            }
            else
            {
                file = inputFileModel;
            }

            var nameExists = true;
            var name = file.FileName;

            while (nameExists)
            {
                // check if folder has " (x) "
                var match = Regex.Match(name, @"(.+)\((\d+)\)");

                // if not add (1)
                if (!match.Success)
                {
                    name += " (1)";
                }
                else
                {
                    // else add (x+1)
                    int.TryParse(match.Groups[2].Value, out int currentCount);

                    name = name.Replace($"({currentCount})", $"({currentCount + 1})");
                }

                if (!filesInFolder.Any(f => f.FileName.Equals(name) && f.FileType.Equals(file.FileType)))
                {
                    nameExists = false;
                    file.FileName = name;
                }
            }
        }

        private List<int> GetSubfolderId(FolderServiceModel model)
        {
            var folderIds = new List<int>();

            if (model.SubFolders.Any())
            {
                foreach (var sub in model.SubFolders)
                {
                    if (sub.SubFolders.Any())
                    {
                        folderIds.AddRange(GetSubfolderId(sub));
                    }

                    folderIds.Add(sub.FolderId);
                }
            }

            return folderIds;
        }

        private async Task<bool> CreateEmptyWordDocAsync(int catalogId, int employeeId, int folderId)
        {
            using (var ms = new MemoryStream())
            {
                return await this.UploadFileAsync(new InputFileServiceModel
                {
                    Chunk = 0,
                    Chunks = 1,
                    FileName = $"Document",
                    FileType = ".docx",
                    Filesize = 0,
                    CatalogId = catalogId,
                    EmployeeId = employeeId,
                    FolderId = folderId,
                    ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    Stream = ms,
                    CreateDate = DateTime.UtcNow
                });
            }
        }

        private async Task<bool> CreateEmptyExcelDocAsync(int catalogId, int employeeId, int folderId)
        {
            using (var ms = this.GenerateEmptyExcelFileInStream())
            {
                return await this.UploadFileAsync(new InputFileServiceModel
                {
                    Chunk = 0,
                    Chunks = 1,
                    FileName = $"Excel",
                    FileType = ".xlsx",
                    Filesize = ms.Length,
                    CatalogId = catalogId,
                    EmployeeId = employeeId,
                    FolderId = folderId,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Stream = ms,
                    CreateDate = DateTime.UtcNow
                });
            }
        }

        private Stream GenerateEmptyExcelFileInStream()
        {
            var ms = new MemoryStream();

            using (var spreadSheetDocument = SpreadsheetDocument.Create(ms, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                var workBookPart = spreadSheetDocument.AddWorkbookPart();
                workBookPart.Workbook = new Workbook();

                var workSheetPart = workBookPart.AddNewPart<WorksheetPart>();
                workSheetPart.Worksheet = new Worksheet(new SheetData());

                var sheets = spreadSheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
                var sheet = new Sheet
                {
                    Id = spreadSheetDocument.WorkbookPart.GetIdOfPart(workSheetPart),
                    SheetId = 1,
                    Name = "WorkSheet1"
                };
                sheets.Append(sheet);
            }

            ms.Position = 0;
            return ms;
        }
    }
}
