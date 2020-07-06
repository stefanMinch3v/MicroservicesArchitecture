namespace TaskTronic.Drive.Data.DapperRepo
{
    using Dapper;
    using Services.Folders;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using static Sqls;

    internal class FolderDAL : IFolderDAL
    {
        private const string FolderTableName = "[dbo].[Folders]";
        private const string PermissionsTableName = "[dbo].[Permissions]";
        private const string FileTableName = "[dbo].[Files]";

        private readonly IDbConnectionFactory dbConnectionFactory;

        public FolderDAL(IDbConnectionFactory dbConnectionFactory)
            => this.dbConnectionFactory = dbConnectionFactory;

        public async Task<int> CreateFolderAsync(InputFolderServiceModel inputModel)
        {
            var sql = string.Format(FolderSql.ADD, FolderTableName);

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                return await db.ExecuteScalarAsync<int>(sql, inputModel);
            }

        }
        public async Task<bool> RenameFolderAsync(int catId, int folderId, string newFolderName)
        {
            var sql = string.Format(FolderSql.RENAME_FOLDER, FolderTableName);

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                return await db.ExecuteAsync(sql, new { catId, folderId, newFolderName }) > 0;
            }
        }

        public async Task<bool> MoveFolderToNewParentAsync(int catId, int folderToMoveId, int newFolderParentId, string folderName)
        {
            var sql = string.Format(FolderSql.MOVE_FOLDER_TO_NEW_PARENT, FolderTableName);

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                return await (db.ExecuteAsync(sql, new { catId, folderToMoveId, newFolderParentId, folderName })) > 0;
            }
        }

        public async Task<bool> CheckForParentFolderAsync(int folderId)
        {
            var sql = string.Format(FolderSql.CHECK_FOR_FOLDER, FolderTableName);

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                return (await db.QuerySingleOrDefaultAsync<int>(sql, new { folderId })) > 0;
            }
        }

        public async Task<bool> CheckForRootFolderAsync(int folderId)
        {
            var sql = string.Format(FolderSql.CHECK_FOR_FOLDER, FolderTableName);

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                return (await db.QuerySingleOrDefaultAsync<int>(sql, new { folderId })) > 0;
            }
        }

        public async Task<bool> CheckForFolderWithSameNameAsync(string name, int parentFolderId)
        {
            var sql = string.Format(FolderSql.CHECK_FOR_FOLDER_NAME_EXIST_IN_FOLDER, FolderTableName);

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                return await db.QuerySingleOrDefaultAsync<int>(sql, new { name, parentFolderId }) > 0;
            }
        }

        public async Task<FolderServiceModel> GetFolderByIdAsync(int folderId)
        {
            var sql = string.Format(FolderSql.GET_BY_ID, FolderTableName);

            using (var conn = this.dbConnectionFactory.GetSqlConnection)
            {
                return await conn.QuerySingleOrDefaultAsync<FolderServiceModel>(sql, new { folderId });
            }
        }

        public async Task<IEnumerable<FolderServiceModel>> GetFoldersByCatalogIdAsync(int catId)
        {
            var sql = string.Format(FolderSql.GET_FOLDERS_BY_CATID, FolderTableName);

            using (var conn = this.dbConnectionFactory.GetSqlConnection)
            {
                return (await conn.QueryAsync<FolderServiceModel>(sql, new { catId })).AsList();
            }
        }

        public async Task<FolderServiceModel> GetRootFolderByCatalogIdAsync(int catalogId)
        {
            var sql = string.Format(FolderSql.GET_ROOT_FOLDER_BY_CATID, FolderTableName);

            using (var conn = this.dbConnectionFactory.GetSqlConnection)
            {
                return await conn.QuerySingleOrDefaultAsync<FolderServiceModel>(sql, new { catalogId });
            }
        }

        public async Task<IEnumerable<FolderWithAccessServiceModel>> GetFolderTreeAsync(int folderId, int employeeId)
        {
            var sql = string.Format(FolderSql.GET_FOLDER_TREE, FolderTableName, PermissionsTableName, FileTableName);

            using (var conn = this.dbConnectionFactory.GetSqlConnection)
            {
                return (await conn.QueryAsync<FolderWithAccessServiceModel>(sql, new { folderId, employeeId })).AsList();
            }
        }


        public async Task<IEnumerable<FolderServiceModel>> GetSubFoldersAsync(int folderId)
        {
            var sql = string.Format(FolderSql.GET_SUBFOLDERS, FolderTableName);

            using (var conn = this.dbConnectionFactory.GetSqlConnection)
            {
                return (await conn.QueryAsync<FolderServiceModel>(sql, new { folderId })).AsList();
            }
        }

        public async Task<bool> IsFolderPrivateAsync(int folderId)
        {
            var sql = string.Format(FolderSql.CHECK_IF_FOLDER_IS_PRIVATE, FolderTableName);

            using (var conn = this.dbConnectionFactory.GetSqlConnection)
            {
                return (await conn.QuerySingleOrDefaultAsync<int>(sql, new { folderId })) > 0;
            }
        }

        public async Task<bool> DeleteAsync(int catId, int folderId)
        {
            var sql = string.Format(FolderSql.DELETE_FOLDER, FolderTableName);

            using (var conn = this.dbConnectionFactory.GetSqlConnection)
            {
                return (await conn.ExecuteAsync(sql, new { folderId, catId })) > 0;
            }
        }

        public async Task<int> CountFoldersForEmployeeAsync(int employeeId)
        {
            var sql = string.Format(FolderSql.COUNT_FOLDERS_FOR_EMPLOYEE, FolderTableName);

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                return await db.ExecuteScalarAsync<int>(sql, new { employeeId });
            }
        }

        public async Task<IReadOnlyCollection<OutputFolderFlatServiceModel>> GetAllFlatForEmployeeAsync(int employeeId)
        {
            var sql = string.Format(FolderSql.FLAT_FOLDERS_FOR_INITIATOR, FolderTableName);

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                return (await db.QueryAsync<OutputFolderFlatServiceModel>(sql, new { employeeId })).AsList();
            }
        }

        public async Task<int?> GetRootFolderIdAsync(int folderId)
        {
            var sql = string.Format(FolderSql.GET_ROOT_FOLDER_ID, FolderTableName);

            using (var conn = this.dbConnectionFactory.GetSqlConnection)
            {
                return await conn.QuerySingleOrDefaultAsync<int?>(sql, new { folderId });
            }
        }

        public async Task<IList<FolderSearchServiceModel>> GetAllForSearchAsync(int catalogId, int? rootFolderId)
        {
            var sql = string.Format(FolderSql.GET_ALL_FLAT_FOR_SEARCH, FolderTableName);

            using (var conn = this.dbConnectionFactory.GetSqlConnection)
            {
                return (await conn.QueryAsync<FolderSearchServiceModel>(sql, new
                {
                    catalogId,
                    rootFolderId
                })).AsList();
            }
        }
    }
}
