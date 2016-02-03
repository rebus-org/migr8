using System.Linq;
using Migr8.Internals;
using NUnit.Framework;

namespace Migr8.Test.Basic
{
    [TestFixture]
    public class SimpleScenario : FixtureBase
    {
        DatabaseMigratorCore _migrator;

        protected override void SetUp()
        {
            _migrator = new DatabaseMigratorCore(new ThreadPrintingConsoleWriter(), TestConfig.ConnectionString);
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

            Assert.That(tableNames, Is.EqualTo(new[] { "Table1", "Table2", "Table3" }));
        }

        [Test]
        public void PicksUpFromWhereItLeft()
        {
            var allMigrations = new[]
            {
                new TestMigration(1, "test", "CREATE TABLE [Table1] ([Id] int)"),
                new TestMigration(2, "test", "CREATE TABLE [Table2] ([Id] int)"),
                new TestMigration(3, "test", "CREATE TABLE [Table3] ([Id] int)"),
            };

            _migrator.Execute(allMigrations.Take(2));

            var tableNameAfterFirstTwoMigrations = GetTableNames();

            _migrator.Execute(allMigrations);

            var tableNameAfterAllMigrations = GetTableNames();

            Assert.That(tableNameAfterFirstTwoMigrations, Is.EqualTo(new[] { "Table1", "Table2" }));
            Assert.That(tableNameAfterAllMigrations, Is.EqualTo(new[] { "Table1", "Table2", "Table3" }));
        }
    }
}