using System.Reflection;
using Migr8.Internal;
using NUnit.Framework;
using Shouldly;
using System.Linq;

namespace Migr8.Test.AssemblyScanner
{
    [TestFixture]
    public class when_scanning_assembly : FixtureFor<Internal.AssemblyScanner>
    {
        protected override Internal.AssemblyScanner SetUp()
        {
            return new Internal.AssemblyScanner(Assembly.GetExecutingAssembly());
        }

        [Test]
        public void assembly_attributes_are_picked_up()
        {
            // arrange
            

            // act
            var migrations = sut.GetAllMigrations().ToList();

            // assert
            migrations.Count.ShouldBe(2);
            migrations.ShouldContain(m => m.TargetDatabaseVersion == 1);
            migrations.ShouldContain(m => m.TargetDatabaseVersion == 2);
        }

        [Test]
        public void using_go_results_in_multiple_sql_statements()
        {
            // arrange


            // act
            var migrations = sut.GetAllMigrations().ToList();

            // assert
            var firstMigration = migrations.Single(m => m.TargetDatabaseVersion == 1);
            firstMigration.Description.ShouldBe("This is my first migration");
            var firstMigrationSqlStatementsTrimmed = firstMigration.SqlStatements.Select(s => s.Trim());
            firstMigrationSqlStatementsTrimmed.Count().ShouldBe(1);
            firstMigrationSqlStatementsTrimmed.ShouldContain("blah!");

            var secondMigration = migrations.Single(m => m.TargetDatabaseVersion == 2);
            secondMigration.Description.ShouldBe("This is the next migration");
            var secondMigrationSqlStatementsTrimmed = secondMigration.SqlStatements.Select(s => s.Trim());
            secondMigrationSqlStatementsTrimmed.Count().ShouldBe(2);
            secondMigrationSqlStatementsTrimmed.ShouldContain("hello world!");
            secondMigrationSqlStatementsTrimmed.ShouldContain("another sql statement");
        }
    }

    [Migration(1, "This is my first migration")]
    class CreateSomeIndex : ISqlMigration
    {
        public string Sql
        {
            get { return "blah!"; }
        }
    }

    [Migration(2, "This is the next migration")]
    class CreateAnotherIndex : ISqlMigration
    {
        public string Sql
        {
            get { return @"hello world!

GO;

another sql statement

go"; }
        }
    }
}