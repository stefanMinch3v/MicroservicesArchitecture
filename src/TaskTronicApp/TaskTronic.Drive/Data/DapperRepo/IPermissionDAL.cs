namespace TaskTronic.Drive.Data.DapperRepo
{
    using Services.Folders;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IPermissionsDAL
    {
        Task CreateFolderPermissionsAsync(int folderId, InputFolderServiceModel inputModel);

        Task<IEnumerable<int>> GetUserFolderPermissionsAsync(int catalogId, int employeeId);

        Task<bool> HasUserPermissionForFolderAsync(int catalogId, int folderId, int employeeId);
    }
}
