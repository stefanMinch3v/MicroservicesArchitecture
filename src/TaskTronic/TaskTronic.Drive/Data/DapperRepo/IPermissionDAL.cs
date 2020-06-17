namespace TaskTronic.Drive.Data.DapperRepo
{
    using Services.Folders;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IPermissionsDAL
    {
        Task CreateFolderPermissionsAsync(int folderId, InputFolderServiceModel inputModel);

        Task<IEnumerable<int>> GetUserFolderPermissionsAsync(int catId, int employeeId);

        Task<bool> HasUserPermissionForFolderAsync(int catId, int folderId, int employeeId);
    }
}
