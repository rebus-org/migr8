using Migr8.Internals;

namespace Migr8.Test.Basic
{
    class TestMigration : IExecutableSqlMigration, ISqlMigration
    {
        public TestMigration(int sequenceNumber, string branchSpecification, string sql, string description = null)
        {
            SequenceNumber = sequenceNumber;
            BranchSpecification = branchSpecification;
            Id = $"{sequenceNumber}-{branchSpecification}";
            Sql = sql;
            Description = description ?? "";

            SqlMigration = this;
        }

        public int SequenceNumber { get; }
        public string BranchSpecification { get; }
        public ISqlMigration SqlMigration { get; }
        public string Id { get; }
        public string Sql { get; }
        public string Description { get; }
    }
}