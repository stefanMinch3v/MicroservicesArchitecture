namespace TaskTronic.Drive.Services.Folders
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IFolderService
    {
        Task<bool> CreateFolderAsync(InputFolderServiceModel inputModel);

        Task<bool> RenameFolderAsync(int catId, int folderId, int employeeId, string newFolderName);
 
        Task<bool> MoveFolderAsync(int catId, int folderId, int employeeId, int newParentFolderId);

        Task<FolderServiceModel> GetFolderByIdAsync(int folderId, int employeeId);

        Task<FolderServiceModel> GetRootFolderByCatIdAsync(int catId, int employeeId, bool includeSubfolders = true);

        Task<IReadOnlyCollection<FolderServiceModel>> GetFoldersByCatIdAsync(int catId, int employeeId);

        Task<bool> DeleteFolderAsync(int catId, int employeeId, int folderId);

        Task<bool> IsFolderPrivateAsync(int folderId);

        Task<FolderServiceModel> GetAccessableFolders(int catId, int employeeId);

        Task<int> CountFoldersAsync(int employeeId);

        Task<IReadOnlyCollection<OutputFolderFlatServiceModel>> GetAllForEmployeeAsync(int employeeId);
    }
}
