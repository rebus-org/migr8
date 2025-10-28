namespace Migr8.Internals
{
    public interface IDb
    {
        IExclusiveDbConnection GetExclusiveDbConnection(string connectionString, Options options, IWriter writer, bool useTransaction = true);
    }
}