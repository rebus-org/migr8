using Migr8.Internals;

namespace Migr8.SqlServer
{
    class SqlServerDb : IDb
    {
        public IExclusiveDbConnection GetExclusiveDbConnection(string connectionString, Options options, bool useTransaction = true)
        {
            return new SqlServerExclusiveDbConnection(connectionString, options, useTransaction);
        }
    }
}