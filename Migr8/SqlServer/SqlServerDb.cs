using Migr8.Internals;

namespace Migr8.SqlServer
{
    class SqlServerDb : IDb
    {
        public IExclusiveDbConnection GetExclusiveDbConnection(string connectionString, Options options)
        {
            return new SqlServerExclusiveDbConnection(connectionString, options);
        }
    }
}