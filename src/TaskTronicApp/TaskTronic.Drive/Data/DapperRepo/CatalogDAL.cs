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

        public async Task<int> GetAsync(int companyDepartmentsId)
        {
            var sql = string.Format(CatalogSql.GET, CatalogTableName);

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                return await db.QuerySingleOrDefaultAsync<int>(sql, new { companyDepartmentsId });
            }
        }

        public async Task<int> AddAsync(int companyDepartmentsId)
        {
            var sql = string.Format(CatalogSql.ADD, CatalogTableName);

            using (var db = this.dbConnectionFactory.GetSqlConnection)
            {
                return await db.ExecuteScalarAsync<int>(sql, new { companyDepartmentsId });
            }
        }
    }
}
