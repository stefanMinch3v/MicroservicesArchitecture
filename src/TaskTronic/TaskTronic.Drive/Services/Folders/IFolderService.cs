namespace TaskTronic.Drive.Services.Folders
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IFolderService
    {
        Task<bool> CreateFolderAsync(InputFolderServiceModel inputModel);

        Task<bool> RenameFolderAsync(int catalogId, int folderId, int employeeId, string newFolderName);
 
        Task<bool> MoveFolderAsync(int catalogId, int folderId, int employeeId, int newParentFolderId);

        Task<FolderServiceModel> GetFolderByIdAsync(int folderId, int employeeId);

        Task<FolderServiceModel> GetRootFolderByCatalogIdAsync(
            int catalogId, 
            int employeeId,
            bool includeSubfolders = true);

        Task<IReadOnlyCollection<FolderServiceModel>> GetFoldersByCatalogIdAsync(int catalogId, int employeeId);

        Task<bool> DeleteFolderAsync(int catalogId, int employeeId, int folderId);

        Task<bool> IsFolderPrivateAsync(int folderId);

        Task<FolderServiceModel> GetAccessableFolders(int catalogId, int employeeId);

        Task<int> CountFoldersAsync(int employeeId);

        Task<IReadOnlyCollection<OutputFolderFlatServiceModel>> GetAllForEmployeeAsync(int employeeId);
    }
}
