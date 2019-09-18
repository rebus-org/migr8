using System.Linq;
using Migr8.Internals;
using NUnit.Framework;

namespace Migr8.Test.Basic
{
    [TestFixture]
    public class SimpleScenarioWithCustomConnectionFactory : FixtureBase
    {
        [Test]
        public void DoesNothingWhenExecutingEmptyListOfMigrations()
        {
            Database.Migrate(TestConfig.ConnectionString, new Migrations(Enumerable.Empty<IExecutableSqlMigration>()));
        }

        [Test]
        public void CanExecuteThreeMigrations()
        {
            var migrations = new[]
            {
                new TestMigration(1, "test", "CREATE TABLE [Table1] ([Id] int)"),
                new TestMigration(2, "test", "CREATE TABLE [Table2] ([Id] int)"),
                new TestMigration(3, "test", "CREATE TABLE [Table3] ([Id] int)"),
            };

            Database.Migrate(TestConfig.ConnectionString, new Migrations(migrations));

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

            Database.Migrate(TestConfig.ConnectionString, new Migrations(allMigrations.Take(2)));

            var tableNameAfterFirstTwoMigrations = GetTableNames();

            Database.Migrate(TestConfig.ConnectionString, new Migrations(allMigrations));

            var tableNameAfterAllMigrations = GetTableNames();

            Assert.That(tableNameAfterFirstTwoMigrations, Is.EqualTo(new[] { "Table1", "Table2" }));
            Assert.That(tableNameAfterAllMigrations, Is.EqualTo(new[] { "Table1", "Table2", "Table3" }));
        }
    }
}