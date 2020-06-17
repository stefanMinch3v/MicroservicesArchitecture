namespace TaskTronic.Drive.Data.DapperRepo
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TaskTronic.Drive.Services.Folders;

    public interface IPermissionsDAL
    {
        Task CreateFolderPermissionsAsync(int folderId, InputFolderServiceModel inputModel);

        Task<IEnumerable<int>> GetUserFolderPermissionsAsync(int catId, string userId);

        Task<bool> HasUserPermissionForFolderAsync(int catId, int folderId, string userId);

        Task<string> GetUsernameByUserIdAsync(string userId);
    }
}
