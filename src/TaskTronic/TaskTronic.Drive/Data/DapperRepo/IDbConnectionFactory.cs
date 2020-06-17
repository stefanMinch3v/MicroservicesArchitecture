namespace TaskTronic.Drive.Data.DapperRepo
{
    using Microsoft.Data.SqlClient;

    public interface IDbConnectionFactory
    {
        SqlConnection GetSqlConnection();
    }
}
