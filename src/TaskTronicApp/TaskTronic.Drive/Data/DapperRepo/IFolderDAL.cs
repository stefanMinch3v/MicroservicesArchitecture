namespace TaskTronic.Drive.Data.DapperRepo
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TaskTronic.Drive.Models.Folders;

    public interface IFolderDAL
    {
        Task<bool> RenameFolderAsync(int catalogId, int folderId, string newFolderName);

        Task<FolderServiceModel> GetFolderByIdAsync(int folderId);

        Task<OutputFolderFlatServiceModel> GetFolderFlatByIdAsync(int folderId);

        Task<FolderServiceModel> GetRootFolderByCatalogIdAsync(int catalogId);

        Task<int?> GetRootFolderIdAsync(int folderId);

        Task<IEnumerable<FolderServiceModel>> GetSubFoldersAsync(int folderId);

        Task<bool> IsFolderPrivateAsync(int folderId);

        Task<IEnumerable<FolderWithAccessServiceModel>> GetFolderTreeAsync(int folderId, int employeeId);

        Task<int> GetFolderNumbersWithExistingNameAsync(string name, int parentFolderId);

        Task<int> CountFoldersForEmployeeAsync(int employeeId);

        Task<IReadOnlyCollection<OutputFolderFlatServiceModel>> GetAllFlatForEmployeeAsync(int employeeId);

        Task<IList<FolderSearchServiceModel>> GetAllForSearchAsync(int catalogId, int? rootFolderId);

        // transactions
        Task<(bool Success, int MessageId)> DeleteAsync(int catalogId, int folderId);
        Task<(int FolderId, int MessageId)> CreateFolderAsync(InputFolderServiceModel inputModel);
    }
}
