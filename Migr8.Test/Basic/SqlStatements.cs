using NUnit.Framework;

namespace Migr8.Test.Basic
{
    [TestFixture]
    public class SqlStatements : FixtureBase
    {
        DatabaseMigratorCore _migrator;

        protected override void SetUp()
        {
            _migrator = new DatabaseMigratorCore(new ConsoleWriter(), TestConfig.ConnectionString);
        }

        [Test]
        public void CanHandleMigrationWithGo_Trivial()
        {
            const string sql = @"

CREATE TABLE [Table1] ([Id] int)

GO

CREATE TABLE [Table2] ([Id] int)
";
            var migrations = new[] { new TestMigration(1, "test", sql) };

            _migrator.Execute(migrations);

            var tableNames = GetTableNames();

            Assert.That(tableNames, Is.EqualTo(new[] { "Table1", "Table2" }));
        }

        [TestCase("go")]
        [TestCase("gO")]
        [TestCase("Go")]
        [TestCase("GO")]
        public void CanHandleMigrationWithGo_IgnoresCase(string go)
        {
            var sql = $@"

CREATE TABLE [Table1] ([Id] int)

{go}

CREATE TABLE [Table2] ([Id] int)
";
            var migrations = new[] { new TestMigration(1, "test", sql) };

            _migrator.Execute(migrations);

            var tableNames = GetTableNames();

            Assert.That(tableNames, Is.EqualTo(new[] { "Table1", "Table2" }));
        }

        [Test]
        public void CanHandleMigrationWithGo_IgnoresWhitespace()
        {
            const string sql = @"

CREATE TABLE [Table1] ([Id] int)

        GO

CREATE TABLE [Table2] ([Id] int)
";
            var migrations = new[] { new TestMigration(1, "test", sql) };

            _migrator.Execute(migrations);

            var tableNames = GetTableNames();

            Assert.That(tableNames, Is.EqualTo(new[] { "Table1", "Table2" }));
        }

        [Test]
        public void CanHandleMigrationWithGo_OnlySplitsOnGoLine()
        {
            const string sql = @"

CREATE TABLE [Table1] ([Id] int, [Go] nvarchar(100))

";
            var migrations = new[] { new TestMigration(1, "test", sql) };

            _migrator.Execute(migrations);

            var tableNames = GetTableNames();

            Assert.That(tableNames, Is.EqualTo(new[] { "Table1" }));
        }

    }
}