namespace TaskTronic.Drive.Services.Folders
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IFolderService
    {
        Task<bool> CreateFolderAsync(InputFolderServiceModel inputModel);

        Task<bool> RenameFolderAsync(int catId, int folderId, string userId, string newFolderName);
 
        Task<bool> MoveFolderAsync(int catId, int folderId, string userId, int newParentFolderId);

        Task<FolderServiceModel> GetFolderByIdAsync(int folderId, string userId);

        Task<FolderServiceModel> GetRootFolderByCatIdAsync(int catId, string userId, bool includeSubfolders = true);

        Task<IReadOnlyCollection<FolderServiceModel>> GetFoldersByCatIdAsync(int catId, string userId);

        Task<bool> DeleteFolderAsync(int catId, string userId, int folderId);

        Task<bool> IsFolderPrivateAsync(int folderId);

        Task<FolderServiceModel> GetAccessableFolders(int catId, string userId);
    }
}
