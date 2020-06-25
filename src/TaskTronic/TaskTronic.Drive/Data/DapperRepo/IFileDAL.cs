namespace TaskTronic.Drive.Data.DapperRepo
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using TaskTronic.Drive.Services.Files;

    public interface IFileDAL
    {
        Task ReadStreamFromFileAsync(int blobId, Stream stream);

        Task<IEnumerable<FileServiceModel>> GetFilesByFolderIdAsync(int folderId);

        Task<FileServiceModel> GetFileByIdAsync(int catId, int folderId, int fileId);

        Task<bool> DeleteFileAsync(int catId, int folderId, int fileId, int blobId);

        Task<bool> RenameFileAsync(int catId, int folderId, int fileId, string newFileName);

        Task<bool> CreateBlobAsync(InputFileServiceModel file);

        Task<bool> AppendChunkToBlobAsync(InputFileServiceModel file);

        Task<int?> DoesFileWithSameNameExistInFolder(int catId, int folderId, string fileName, string fileType);

        Task<OutputFileDownloadServiceModel> GetFileInfoForDownloadAsync(int fileId);

        Task<bool> MoveFileAsync(int catId, int folderId, int fileId, int newFolderId, string fileName);

        Task<IEnumerable<FileServiceModel>> SearchFilesAsync(int catId, string value);

        Task<int> CountFilesForEmployeeAsync(int employeeId);

        // transaction
        Task<int> SaveCompletedUploadAsync(InputFileServiceModel file);
        Task<int> SaveCompletedUploadAsync(InputFileServiceModel file, string oldFileName);
        Task<int> SaveCompletedUploadAsReplaceExistingFileAsync(InputFileServiceModel file, int fileId);
    }
}
