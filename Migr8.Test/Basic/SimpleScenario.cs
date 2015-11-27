using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;

namespace Migr8.Test.Basic
{
    [TestFixture]
    public class SimpleScenario : FixtureBase
    {
        DatabaseMigratorCore _migrator;

        protected override void SetUp()
        {
            _migrator = new DatabaseMigratorCore(new ConsoleWriter(),  TestConfig.ConnectionString);
        }

        [Test]
        public void DoesNothingWhenExecutingEmptyListOfMigrations()
        {
            _migrator.Execute(Enumerable.Empty<IExecutableSqlMigration>());
        }

        [Test]
        public void CanExecuteThreeMigrations()
        {
            _migrator.Execute(new[]
            {
                new TestMigration(1, "test", "CREATE TABLE [Table1] ([Id] int)"),
                new TestMigration(2, "test", "CREATE TABLE [Table2] ([Id] int)"),
                new TestMigration(3, "test", "CREATE TABLE [Table3] ([Id] int)"),
            });

            var tableNames = GetTableNames();

            Assert.That(tableNames, Is.EqualTo(new[] {"Table1", "Table2", "Table3"}));
        }

        class TestMigration : IExecutableSqlMigration
        {
            public TestMigration(int number, string sequenceId, string sql)
            {
                Number = number;
                Id = $"{number}-{sequenceId}";
                Sql = sql;
            }

            public int Number { get;}
            public string Id { get; }
            public string Sql { get;  }
        }
    }
}