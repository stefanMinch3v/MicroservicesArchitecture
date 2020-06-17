namespace TaskTronic.Drive.Data.DapperRepo
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TaskTronic.Drive.Services.Folders;

    public interface IFolderDAL
    {
        Task<int> CreateFolderAsync(InputFolderServiceModel inputModel);

        Task<bool> RenameFolderAsync(int catId, int folderId, string newFolderName);

        Task<FolderServiceModel> GetFolderByIdAsync(int folderId);

        Task<IEnumerable<FolderServiceModel>> GetFoldersByCatIdAsync(int catId);

        Task<FolderServiceModel> GetRootFolderByCatIdAsync(int catId);

        Task<IEnumerable<FolderServiceModel>> GetSubFoldersAsync(int folderId);

        Task<bool> CheckForParentFolderAsync(int folderId);

        Task<bool> CheckForRootFolderAsync(int folderId);

        Task<bool> IsFolderPrivateAsync(int folderId);

        Task<IEnumerable<FolderWithAccessServiceModel>> GetFolderTreeAsync(int folderId, int employeeId);

        Task<bool> MoveFolderToNewParentAsync(int catId, int folderToMoveId, int newFolderParentId, string folderName);

        Task<bool> CheckForFolderWithSameNameAsync(string name, int parentFolderId);

        Task<bool> DeleteAsync(int catId, int folderId);
    }
}
