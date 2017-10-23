namespace Migr8.Internals
{
    interface IDb
    {
        IExclusiveDbConnection GetExclusiveDbConnection(string connectionString, Options options);
    }
}