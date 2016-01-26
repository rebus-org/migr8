namespace Migr8.Internals.Databases
{
    class SqlServerDb : IDb
    {
        public IExclusiveDbConnection GetExclusiveDbConnection(string connectionString)
        {
            return new SqlServerExclusiveDbConnection(connectionString);
        }
    }
}