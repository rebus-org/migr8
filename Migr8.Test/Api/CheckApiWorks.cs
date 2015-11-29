using NUnit.Framework;

namespace Migr8.Test.Api
{
    [TestFixture]
    public class CheckApiWorks : FixtureBase
    {
        [Test]
        public void ItWorks()
        {
            Database.Migrate(TestConfig.ConnectionString, Migrations.FromThisAssembly());

            var tableNames = GetTableNames();

            Assert.That(tableNames, Is.EqualTo(new[] {"MyFirstTable", "MySecondTable"}));
        }
    }

    [Migration(1, "This is my first migration", "master")]
    class CreateMyFirstTable : ISqlMigration
    {
        public string Sql => @"CREATE TABLE [MyFirstTable] ([Id] int)";
    }

    [Migration(2, "This is my second migration", "feature-1")]
    class CreateMySecondTable : ISqlMigration
    {
        public string Sql => @"CREATE TABLE [MySecondTable] ([Id] int)";
    }

    [Migration(2, "This is my second migration", "feature-2")]
    class AddColumnToMyFirstTable : ISqlMigration
    {
        public string Sql => @"ALTER TABLE [MyFirstTable] ADD [Text] NVARCHAR(100) NULL";
    }

    [Migration(3, "This is my third migration", "master")]
    class AddColumnsToBothTables : ISqlMigration
    {
        public string Sql => @"

ALTER TABLE [MyFirstTable] ADD [MoreText] NVARCHAR(100) NULL

GO

ALTER TABLE [MySecondTable] ADD [MoreText] NVARCHAR(100) NULL

";
    }
}