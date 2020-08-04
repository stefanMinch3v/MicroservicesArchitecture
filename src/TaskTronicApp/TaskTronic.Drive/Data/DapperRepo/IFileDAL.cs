﻿namespace TaskTronic.Drive.Data.DapperRepo
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using TaskTronic.Drive.Models.Files;

    public interface IFileDAL
    {
        Task ReadStreamFromFileAsync(int blobId, Stream stream);

        Task<IEnumerable<FileServiceModel>> GetFilesByFolderIdAsync(int folderId);

        Task<FileServiceModel> GetFileByIdAsync(int catalogId, int folderId, int fileId);

        Task<bool> RenameFileAsync(int catalogId, int folderId, int fileId, string newFileName);

        Task<bool> CreateBlobAsync(InputFileServiceModel file);

        Task<int?> DoesFileWithSameNameExistInFolder(int catalogId, int folderId, string fileName, string fileType);

        Task<OutputFileDownloadServiceModel> GetFileInfoForDownloadAsync(int fileId);

        Task<IEnumerable<FileServiceModel>> SearchFilesAsync(int catalogId, string value, IEnumerable<int> accessibleFiles);

        Task<int> CountFilesForEmployeeAsync(int employeeId);

        // transactions
        Task<(bool Success, int MessageId)> DeleteFileAsync(int catalogId, int folderId, int fileId, int blobId);
        Task<(int FileId, int MessageId)> SaveCompletedUploadAsync(InputFileServiceModel file);
        Task<(int FileId, int MessageId)> SaveCompletedUploadAsync(InputFileServiceModel file, string oldFileName);
        Task<bool> AppendChunkToBlobAsync(InputFileServiceModel file);
    }
}
