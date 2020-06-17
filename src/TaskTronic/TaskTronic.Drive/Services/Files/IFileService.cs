namespace TaskTronic.Drive.Services.Files
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public interface IFileService
    {
        Task ReadStreamFromFileAsync(int blobId, Stream stream);

        Task<FileServiceModel> GetFileByIdAsync(int catId, int folderId, int fileId);

        Task<IReadOnlyCollection<FileServiceModel>> GetFilesByFolderIdAsync(int catId, int folderId, int employeeId);

        Task<bool> DeleteFileAsync(int employeeId, int catId, int folderId, int fileId);

        Task<bool> UploadFileAsync(InputFileServiceModel file);

        Task<Dictionary<string, bool>> CheckFilesInFolderForCollisions(int catId, int employeeId, int folderId, string[] fileNames);

        Task<OutputFileDownloadServiceModel> GetFileInfoForDownloadAsync(int catId, int employeeId, int folderId, int fileId);

        Task<bool> RenameFileAsync(int catId, int folderId, int fileId, int employeeId, string newFileName);

        Task<bool> MoveFileAsync(int catId, int folderId, int fileId, int newFolderId, int employeeId);

        Task<IReadOnlyCollection<OutputFileServiceModel>> SearchFilesAsync(int catId, int employeeId, string value);

        Task<bool> CreateNewFileAsync(int catId, int employeeId, int folderId, NewFileType fileType);
    }
}
