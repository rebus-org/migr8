using Migr8.Internals;

namespace Migr8.Test.Basic
{
    class TestMigration : IExecutableSqlMigration
    {
        public TestMigration(int number, string sequenceId, string sql, string description = null)
        {
            Number = number;
            Id = $"{number}-{sequenceId}";
            Sql = sql;
            Description = description ?? "";
        }

        public int Number { get; }
        public string Id { get; }
        public string Sql { get; }
        public string Description { get; }
    }
}