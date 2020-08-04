namespace TaskTronic.Drive.Data.DapperRepo
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TaskTronic.Drive.Models.Folders;

    public interface IPermissionsDAL
    {
        Task CreateFolderPermissionsAsync(int folderId, InputFolderServiceModel inputModel);

        Task<IEnumerable<int>> GetUserFolderPermissionsAsync(int catalogId, int employeeId);

        Task<bool> HasUserPermissionForFolderAsync(int catalogId, int folderId, int employeeId);
    }
}
