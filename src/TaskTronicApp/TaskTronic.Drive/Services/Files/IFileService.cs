namespace TaskTronic.Drive.Services.Files
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public interface IFileService
    {
        Task ReadStreamFromFileAsync(int blobId, Stream stream);

        Task<FileServiceModel> GetFileByIdAsync(int catalogId, int folderId, int fileId);

        Task<IReadOnlyCollection<FileServiceModel>> GetFilesByFolderIdAsync(int catalogId, int folderId, int employeeId);

        Task<bool> DeleteFileAsync(int employeeId, int catalogId, int folderId, int fileId);

        Task<bool> UploadFileAsync(InputFileServiceModel file);

        Task<Dictionary<string, bool>> CheckFilesInFolderForCollisions(int catalogId, int employeeId, int folderId, string[] fileNames);

        Task<OutputFileDownloadServiceModel> GetFileInfoForDownloadAsync(int catalogId, int employeeId, int folderId, int fileId);

        Task<bool> RenameFileAsync(int catalogId, int folderId, int fileId, int employeeId, string newFileName);

        Task<bool> MoveFileAsync(int catalogId, int folderId, int fileId, int newFolderId, int employeeId);

        Task<bool> CreateNewFileAsync(int catalogId, int employeeId, int folderId, NewFileType fileType);

        Task<int> CountFilesAsync(int employeeId);
    }
}
