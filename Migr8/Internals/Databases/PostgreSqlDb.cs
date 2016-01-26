namespace Migr8.Internals.Databases
{
    class PostgreSqlDb : IDb
    {
        public IExclusiveDbConnection GetExclusiveDbConnection(string connectionString)
        {
            return new PostgresqlExclusiveDbConnection(connectionString);
        }
    }
}