using Migr8.Internals;

namespace Migr8.Postgres
{
    class PostgreSqlDb : IDb
    {
        public IExclusiveDbConnection GetExclusiveDbConnection(string connectionString)
        {
            return new PostgresqlExclusiveDbConnection(connectionString);
        }
    }
}