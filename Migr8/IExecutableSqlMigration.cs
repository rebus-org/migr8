namespace Migr8
{
    interface IExecutableSqlMigration
    {
        string Id { get; }
        string Sql { get; }
    }
}