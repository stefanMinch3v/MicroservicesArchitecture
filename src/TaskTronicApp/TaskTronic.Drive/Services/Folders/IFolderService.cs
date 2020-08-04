namespace TaskTronic.Drive.Services.Folders
{
    using Models.Files;
    using Models.Folders;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IFolderService
    {
        Task<bool> CreateFolderAsync(InputFolderServiceModel inputModel);

        Task<bool> RenameFolderAsync(int catalogId, int folderId, int employeeId, string newFolderName);

        Task<FolderServiceModel> GetFolderByIdAsync(int folderId, int employeeId, int? companyDepartmentsId = null);

        Task CheckFolderPermissionsAsync(int catalogId, int folderId, int employeeId);

        Task<FolderServiceModel> GetRootFolderByCatalogIdAsync(
            int catalogId, 
            int employeeId,
            bool includeSubfolders = true);

        Task<bool> DeleteFolderAsync(int catalogId, int employeeId, int folderId);

        Task<bool> IsFolderPrivateAsync(int folderId);

        Task<IReadOnlyCollection<OutputFolderFlatServiceModel>> GetAllForEmployeeAsync(int employeeId);

        Task<IEnumerable<FileServiceModel>> SearchFilesAsync(
            int catalogId,
            int employeeId,
            int currentFolderId,
            string searchValue);
    }
}
