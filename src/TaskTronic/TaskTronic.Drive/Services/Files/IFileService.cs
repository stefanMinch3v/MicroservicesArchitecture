namespace TaskTronic.Drive.Services.Files
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public interface IFileService
    {
        Task ReadStreamFromFileAsync(int blobId, Stream stream);

        Task<FileServiceModel> GetFileByIdAsync(int catId, int folderId, int fileId);

        Task<IReadOnlyCollection<FileServiceModel>> GetFilesByFolderIdAsync(int catId, int folderId, string userId);

        Task<bool> DeleteFileAsync(string userId, int catId, int folderId, int fileId);

        Task<bool> UploadFileAsync(InputFileServiceModel file);

        Task<Dictionary<string, bool>> CheckFilesInFolderForCollisions(int catId, string userId, int folderId, string[] fileNames);

        Task<OutputFileDownloadServiceModel> GetFileInfoForDownloadAsync(int catId, string userId, int folderId, int fileId);

        Task<bool> RenameFileAsync(int catId, int folderId, int fileId, string userId, string newFileName);

        Task<bool> MoveFileAsync(int catId, int folderId, int fileId, int newFolderId, string userId);

        Task<IReadOnlyCollection<OutputFileServiceModel>> SearchFilesAsync(int catId, string userId, string value);

        Task<bool> CreateNewFileAsync(int catId, string userId, int folderId, NewFileType fileType);
    }
}
