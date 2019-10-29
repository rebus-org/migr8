using Migr8.Internals;

namespace Migr8.Npgsql.Postgres
{
    class PostgreSqlDb : IDb
    {
        public IExclusiveDbConnection GetExclusiveDbConnection(string connectionString, Options options, IWriter writer, bool useTransaction = true)
        {
            return new PostgresqlExclusiveDbConnection(connectionString, options, useTransaction);
        }
    }
}