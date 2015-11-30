namespace Migr8.Internals
{
    interface IExecutableSqlMigration
    {
        string Id { get; }
        string Sql { get; }
        string Description { get; }
        int SequenceNumber { get; }
        string BranchSpecification { get; }
        ISqlMigration SqlMigration { get; }
    }
}