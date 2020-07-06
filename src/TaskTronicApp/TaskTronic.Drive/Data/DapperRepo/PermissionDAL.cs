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

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                await db.ExecuteAsync(sql, new
                {
                    inputModel.CatalogId,
                    folderId,
                    inputModel.EmployeeId
                });
            }
        }

        public async Task<IEnumerable<int>> GetUserFolderPermissionsAsync(int catalogId, int employeeId)
        {
            var sql = string.Format(PermissionsSql.GET_USER_FOLDER_PERMISSIONS, PermissionsTablename);

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                return (await db.QueryAsync<int>(sql, new { employeeId, catalogId })).AsList();
            }
        }

        public async Task<bool> HasUserPermissionForFolderAsync(int catalogId, int folderId, int employeeId)
        {
            var sql = string.Format(PermissionsSql.HAS_USER_PERMISSION_FOR_FOLDER, PermissionsTablename);

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                return await db.QuerySingleAsync<bool>(sql, new { employeeId, catalogId, folderId });
            }
        }
    }
}
