namespace TaskTronic.Drive.Data.DapperRepo
{
    using Dapper;
    using System.Threading.Tasks;

    using static Sqls;

    internal class CatalogDAL : ICatalogDAL
    {
        private const string CatalogTableName = "[dbo].[Catalogs]";
        private readonly IDbConnectionFactory dbConnectionFactory;

        public CatalogDAL(IDbConnectionFactory dbConnectionFactory)
            => this.dbConnectionFactory = dbConnectionFactory;

        public async Task<int?> GetAsync(int companyId, int departmentId)
        {
            var sql = this.GenerateSelectSql();

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                return await db.QuerySingleOrDefaultAsync<int?>(sql, new { companyId, departmentId });
            }
        }

        public async Task<int> AddAsync(int companyId, int departmentId)
        {
            var sql = string.Format(CatalogSql.ADD, CatalogTableName);

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                return await db.ExecuteScalarAsync<int>(sql, new { companyId, departmentId });
            }
        }

        private string GenerateSelectSql()
        {
            return $@"
                SELECT * FROM {CatalogTableName}
                WHERE CompanyId = @CompanyId
                    AND DepartmentId = @DepartmentId";

            // removed old data
        }
    }
}
