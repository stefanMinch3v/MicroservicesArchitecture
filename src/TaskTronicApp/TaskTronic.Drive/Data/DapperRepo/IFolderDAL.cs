namespace TaskTronic.Drive.Data.DapperRepo
{
    using Services.Folders;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IFolderDAL
    {
        Task<int> CreateFolderAsync(InputFolderServiceModel inputModel);

        Task<bool> RenameFolderAsync(int catalogId, int folderId, string newFolderName);

        Task<FolderServiceModel> GetFolderByIdAsync(int folderId);

        Task<IEnumerable<FolderServiceModel>> GetFoldersByCatalogIdAsync(int catalogId);

        Task<FolderServiceModel> GetRootFolderByCatalogIdAsync(int catalogId);

        Task<int?> GetRootFolderIdAsync(int folderId);

        Task<IEnumerable<FolderServiceModel>> GetSubFoldersAsync(int folderId);

        Task<bool> CheckForParentFolderAsync(int folderId);

        Task<bool> CheckForRootFolderAsync(int folderId);

        Task<bool> IsFolderPrivateAsync(int folderId);

        Task<IEnumerable<FolderWithAccessServiceModel>> GetFolderTreeAsync(int folderId, int employeeId);

        Task<bool> MoveFolderToNewParentAsync(int catalogId, int folderToMoveId, int newFolderParentId, string folderName);

        Task<bool> CheckForFolderWithSameNameAsync(string name, int parentFolderId);

        Task<bool> DeleteAsync(int catalogId, int folderId);

        Task<int> CountFoldersForEmployeeAsync(int employeeId);

        Task<IReadOnlyCollection<OutputFolderFlatServiceModel>> GetAllFlatForEmployeeAsync(int employeeId);

        Task<IList<FolderSearchServiceModel>> GetAllForSearchAsync(int catalogId, int? rootFolderId);
    }
}
