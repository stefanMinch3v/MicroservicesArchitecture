namespace TaskTronic.Drive.Data.DapperRepo
{
    using Dapper;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TaskTronic.Drive.Services.Folders;

    using static Sqls;

    internal class PermissionsDAL : IPermissionsDAL
    {
        private const string PermissionsTablename = "[dbo].[Permissions]";
        private readonly IDbConnectionFactory dbConnectionFactory;

        public PermissionsDAL(IDbConnectionFactory dbConnectionFactory)
            => this.dbConnectionFactory = dbConnectionFactory;

        public async Task CreateFolderPermissionsAsync(int folderId, InputFolderServiceModel inputModel)
        {
            var sql = string.Format(PermissionsSql.ADD_SINGLE, PermissionsTablename);

            using (var db = this.dbConnectionFactory.GetSqlConnection())
            {
                await db.ExecuteAsync(sql, new
                {
                    inputModel.CatalogId,
                    folderId,
                    inputModel.UserId
                });
            }
        }

        public async Task<IEnumerable<int>> GetUserFolderPermissionsAsync(int catId, string userId)
        {
            var sql = string.Format(PermissionsSql.GET_USER_FOLDER_PERMISSIONS, PermissionsTablename);

            using (var db = this.dbConnectionFactory.GetSqlConnection())
            {
                return (await db.QueryAsync<int>(sql, new { userId, catId })).AsList();
            }
        }

        public async Task<bool> HasUserPermissionForFolderAsync(int catId, int folderId, string userId)
        {
            var sql = string.Format(PermissionsSql.HAS_USER_PERMISSION_FOR_FOLDER, PermissionsTablename);

            using (var db = this.dbConnectionFactory.GetSqlConnection())
            {
                return await db.QuerySingleAsync<bool>(sql, new { userId, catId, folderId });
            }
        }

        public Task<string> GetUsernameByUserIdAsync(string userId)
        {
            return Task.FromResult($"Unknown user with id: {userId}");
            //var sql = string.Format(PermissionsSql.GET_USERNAME_BY_USER_ID, UserTableName);

            //using (var db = this.usersFactory.CreateConnection())
            //{
            //    var names = await db.QuerySingleOrDefaultAsync<(string username, string first, string last)>(sql, new { userId });

            //    var username = names.username;
            //    var firstName = names.first;
            //    var lastName = names.last;

            //    if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            //    {
            //        return username;
            //    }

            //    return $"{firstName[0]}{lastName[0]}";
            //}
        }
    }
}
