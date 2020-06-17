﻿namespace TaskTronic.Drive.Services.Files
{
    using Data.DapperRepo;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;
    using Exceptions;
    using Services.Folders;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using TaskTronic.Common;

    public class FileService : IFileService
    {
        private readonly IFileDAL fileDAL;
        private readonly IPermissionsDAL permissionsDAL;
        private readonly IFolderService folderService;

        public FileService(
            IFileDAL fileDAL,
            IPermissionsDAL permissionsDAL,
            IFolderService folder)
        {
            this.fileDAL = fileDAL;
            this.permissionsDAL = permissionsDAL;
            this.folderService = folder;
        }

        public async Task<IReadOnlyCollection<FileServiceModel>> GetFilesByFolderIdAsync(int catId, int folderId, string userId)
        {
            var isPrivate = await this.folderService.IsFolderPrivateAsync(folderId);

            if (isPrivate)
            {
                var hasPermission = await this.permissionsDAL.HasUserPermissionForFolderAsync(catId, folderId, userId);
                if (!hasPermission)
                {
                    throw new PermissionException { Message = "You do not have access to this folder." };
                }
            }

            return (await this.fileDAL.GetFilesByFolderIdAsync(folderId)).ToList();
        }

        public async Task<bool> DeleteFileAsync(string userId, int catId, int folderId, int fileId)
        {
            var isPrivate = await this.folderService.IsFolderPrivateAsync(folderId);
            if (isPrivate)
            {
                var hasPermission = await this.permissionsDAL.HasUserPermissionForFolderAsync(catId, folderId, userId);
                if (!hasPermission)
                {
                    throw new PermissionException { Message = "You do not have access to this folder." };
                }
            }

            var file = await this.fileDAL.GetFileByIdAsync(catId, folderId, fileId);
            if (file is null)
            {
                throw new FileException { Message = "File not found." };
            }

            return await this.fileDAL.DeleteFileAsync(file.CatalogId, file.FolderId, file.FileId, file.BlobId);
        }

        public async Task<bool> RenameFileAsync(int catId, int folderId, int fileId, string userId, string newFileName)
        {
            Guard.AgainstEmptyString<FileException>(newFileName, nameof(newFileName));
            Guard.AgainstInvalidWindowsCharacters<FileException>(newFileName, nameof(newFileName));

            var file = await this.fileDAL.GetFileByIdAsync(catId, folderId, fileId);
            if (file is null)
            {
                throw new FileException { Message = "File not found." };
            }

            var folder = await this.folderService.GetFolderByIdAsync(folderId, userId);
            if (folder is null)
            {
                throw new FolderException { Message = "Folder not found." };
            }

            if (folder.IsPrivate)
            {
                var hasPermission = await this.permissionsDAL.HasUserPermissionForFolderAsync(catId, folderId, userId);
                if (!hasPermission)
                {
                    throw new PermissionException { Message = "You dont have access to this folder." };
                }
            }

            return await this.fileDAL.RenameFileAsync(catId, folderId, fileId, newFileName);
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

            var insertedId = 0;
            var existingFileId = await this.fileDAL.DoesFileWithSameNameExistInFolder(file.CatalogId, file.FolderId, file.FileName, file.FileType);
            if (existingFileId.HasValue && existingFileId.Value > 0)
            {
                // file with name does exist in folder
                if (file.ReplaceExistingFiles)
                {
                    // replace the existing file
                    insertedId = await this.fileDAL.SaveCompletedUploadAsReplaceExistingFileAsync(file, existingFileId.Value);
                }
                else
                {
                    // add as new file with new fileName
                    // check if we need to change filename
                    var filesInFolder = await this.GetFilesByFolderIdAsync(file.CatalogId, file.FolderId, file.UserId);

                    if (filesInFolder != null && filesInFolder.Any(f => f.FileName.Equals(file.FileName) && f.FileType.Equals(file.FileType)))
                    {
                        var oldFileName = file.FileName;
                        this.RenameFileName(filesInFolder, inputFileModel: file);

                        file.CreateDate = DateTimeOffset.UtcNow;
                        file.UpdateDate = DateTimeOffset.UtcNow;

                        insertedId = await this.fileDAL.SaveCompletedUploadAsync(file, oldFileName);
                    }
                }

                if (insertedId < 1)
                {
                    return false;
                }
            }
            else // file with name does NOT exist in folder
            {
                file.CreateDate = DateTimeOffset.UtcNow;
                file.UpdateDate = DateTimeOffset.UtcNow;

                insertedId = await this.fileDAL.SaveCompletedUploadAsync(file);
                if (insertedId < 1)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<Dictionary<string, bool>> CheckFilesInFolderForCollisions(int catId, string userId, int folderId, string[] fileNames)
        {
            var result = new Dictionary<string, bool>();

            if (fileNames.Any())
            {
                var folder = await this.folderService.GetFolderByIdAsync(folderId, userId);

                if (folder is null)
                {
                    throw new FolderException { Message = "Folder not found." };
                }

                if (folder.IsPrivate)
                {
                    var hasPermission = await this.permissionsDAL.HasUserPermissionForFolderAsync(catId, folderId, userId);

                    if (!hasPermission)
                    {
                        throw new PermissionException { Message = "You dont have access to this folder." };
                    }
                }

                var filesInFolder = await this.fileDAL.GetFilesByFolderIdAsync(folderId);
                if (filesInFolder.Any())
                {
                    foreach (var fileNameToCheck in fileNames)
                    {
                        result.Add(fileNameToCheck, filesInFolder.Any(x => fileNameToCheck.Equals(x.FileName + x.FileType)));
                    }
                }
                else
                {
                    foreach (var fileNameToCheck in fileNames)
                    {
                        result.Add(fileNameToCheck, false);
                    }
                }
            }

            return result;
        }

        public Task ReadStreamFromFileAsync(int blobId, Stream stream)
            => this.fileDAL.ReadStreamFromFileAsync(blobId, stream);

        public async Task<OutputFileDownloadServiceModel> GetFileInfoForDownloadAsync(int catId, string userId, int folderId, int fileId)
        {
            var isPrivate = await this.folderService.IsFolderPrivateAsync(folderId);

            if (isPrivate)
            {
                var hasPermission = await this.permissionsDAL.HasUserPermissionForFolderAsync(catId, folderId, userId);
                if (!hasPermission)
                {
                    throw new PermissionException { Message = "The folder is private." };
                }
            }

            return await this.fileDAL.GetFileInfoForDownloadAsync(fileId);
        }

        public async Task<bool> MoveFileAsync(int catId, int folderId, int fileId, int newFolderId, string userId)
        {
            var folder = await this.folderService.GetFolderByIdAsync(folderId, userId);

            if (folder is null)
            {
                throw new FolderException { Message = "Folder not found." };
            }

            if (folder.IsPrivate)
            {
                var hasPermission = await this.permissionsDAL.HasUserPermissionForFolderAsync(catId, folderId, userId);
                if (!hasPermission)
                {
                    throw new PermissionException { Message = "The folder is private." };
                }
            }

            var oldFolderId = folder.FolderId;

            var newFolder = await this.folderService.GetFolderByIdAsync(newFolderId, userId);

            if (newFolder is null)
            {
                throw new FolderException { Message = "Folder not found." };
            }

            if (newFolder.IsPrivate)
            {
                var hasPermission = await this.permissionsDAL.HasUserPermissionForFolderAsync(catId, newFolderId, userId);

                if (!hasPermission)
                {
                    throw new PermissionException { Message = "The folder is private." };
                }
            }

            // check if we need to change filename
            var filesInFolder = await this.GetFilesByFolderIdAsync(catId, newFolderId, userId);

            var file = await this.GetFileByIdAsync(catId, folderId, fileId);

            if (filesInFolder != null && filesInFolder.Any(f => f.FileName.Equals(file.FileName)))
            {
                this.RenameFileName(filesInFolder, fileModel: file);
            }

            return await this.fileDAL.MoveFileAsync(catId, folderId, file.FileId, newFolderId, file.FileName);
        }

        public async Task<FileServiceModel> GetFileByIdAsync(int catId, int folderId, int fileId)
        {
            var file = await this.fileDAL.GetFileByIdAsync(catId, folderId, fileId);

            if (file is null)
            {
                throw new FileException { Message = "File not found." };
            }

            file.UpdaterUsername = await this.permissionsDAL.GetUsernameByUserIdAsync(file.UpdaterId);

            return file;
        }

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

        private async Task<List<int>> FindAccessibleFolderIds(string userId, int catId)
        {
            var acessibleFolders = await this.folderService.GetAccessableFolders(catId, userId);

            var folderIds = new List<int>
            {
                acessibleFolders.FolderId
            };

            folderIds.AddRange(GetSubfolderId(acessibleFolders));

            return folderIds;
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


        public async Task<IReadOnlyCollection<OutputFileServiceModel>> SearchFilesAsync(int catId, string userId, string value)
        {
            var matchedFilesTask = this.fileDAL.SearchFilesAsync(catId, value);
            var folderIdsTask = FindAccessibleFolderIds(userId, catId);

            await Task.WhenAll(matchedFilesTask, folderIdsTask);

            var matchedFilesResult = await matchedFilesTask;
            var folderIdsResult = await folderIdsTask;

            matchedFilesResult = matchedFilesResult.Where(x => folderIdsResult.Contains(x.FolderId));
            return matchedFilesResult
                .Select(this.MapToOutputModel)
                .ToArray();
        }

        public async Task<bool> CreateNewFileAsync(int catId, string userId, int folderId, NewFileType fileType)
            => fileType switch
            {
                NewFileType.Word => await this.CreateEmptyWordDocAsync(catId, userId, folderId),
                NewFileType.Excel => await this.CreateEmptyExcelDocAsync(catId, userId, folderId),
                _ => throw new InvalidOperationException($"Unsupported file type: {fileType}"),
            };

        private async Task<bool> CreateEmptyWordDocAsync(int catId, string userId, int folderId)
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
                    CatalogId = catId,
                    UserId = userId,
                    FolderId = folderId,
                    ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    Stream = ms
                });
            }
        }

        public async Task<bool> CreateEmptyExcelDocAsync(int catId, string userId, int folderId)
        {
            using (var ms = this.GenerateEmptyExcelFileInStream())
            {
                return await this.UploadFileAsync(new InputFileServiceModel
                {
                    Chunk = 0,
                    Chunks = 1,
                    FileName = $"Mappe",
                    FileType = ".xlsx",
                    Filesize = ms.Length,
                    CatalogId = catId,
                    UserId = userId,
                    FolderId = folderId,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Stream = ms
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

        private OutputFileServiceModel MapToOutputModel(FileServiceModel model)
        {
            if (model is null)
            {
                return null;
            }

            return new OutputFileServiceModel
            {
                BlobId = model.BlobId,
                CatalogId = model.CatalogId,
                ContentType = model.ContentType,
                FileId = model.FileId,
                FileName = model.FileName,
                FileSize = model.FileSize,
                FileType = model.FileType,
                FolderId = model.FolderId,
                UpdaterId = model.UpdaterId,
                UpdaterUsername = model.UpdaterUsername
            };
        }
    }
}