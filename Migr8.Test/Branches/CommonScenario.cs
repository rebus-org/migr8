using System.Collections.Generic;
using System.Linq;
using Migr8.Internals;
using Migr8.Test.Basic;
using NUnit.Framework;

namespace Migr8.Test.Branches
{
    [TestFixture]
    public class CommonScenario : DbFixtureBase
    {
        DatabaseMigratorCore _migrator;

        protected override void SetUp()
        {
            base.SetUp();
            _migrator = new DatabaseMigratorCore(new ThreadPrintingConsoleWriter(), TestConfig.ConnectionString);
        }

        static readonly TestMigration[] AllMigrations = {
            // 0 - we all got this migration on master
            new TestMigration(1, "master", "CREATE TABLE [Table1] ([Id] int)"),

            // 1 - then someone branched out to 'feature-something' and did these two
            new TestMigration(2, "feature-something", "CREATE TABLE [Table2] ([Id] int)"),
            // 2
            new TestMigration(3, "feature-something", "ALTER TABLE [Table2] ADD [Name] NVARCHAR(MAX) NULL"),

            // 3 - at the same time as someone else branched out to 'feature-anotherthing' and did these:
            new TestMigration(2, "feature-anotherthing", "CREATE TABLE [Table3] ([Id] int)"),
            // 4
            new TestMigration(3, "feature-anotherthing", "ALTER TABLE [Table3] ADD [Name] NVARCHAR(MAX) NULL"),

            // 5 - and then we all came together in master again...
            new TestMigration(4, "master", "CREATE TABLE [Table4] ([Id] int)"),
        };

        [Test]
        public void NizzleName()
        {
            // this is how 'feature-something' will get to execute the migrations - first his own migrations
            _migrator.Execute(Migrations(0, 1, 2).InRandomOrder());

            // then he merges his own stuff into master
            _migrator.Execute(Migrations(0, 1, 2, 5).InRandomOrder());

            // the the 'feature-anotherthing' branch gets merged
            _migrator.Execute(AllMigrations.InRandomOrder());
        }

        IEnumerable<IExecutableSqlMigration> Migrations(params int[] indexes)
        {
            return indexes.Select(i => AllMigrations[i]);
        }
    }
}