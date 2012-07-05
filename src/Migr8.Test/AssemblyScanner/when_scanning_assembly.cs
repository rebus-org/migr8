using System.Reflection;
using NUnit.Framework;
using Shouldly;
using System.Linq;

namespace Migr8.Test.AssemblyScanner
{
    [TestFixture]
    public class when_scanning_assembly : FixtureFor<Migr8.AssemblyScanner>
    {
        protected override Migr8.AssemblyScanner SetUp()
        {
            return new Migr8.AssemblyScanner(Assembly.GetExecutingAssembly());
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

            var firstMigration = migrations.Single(m => m.TargetDatabaseVersion == 1);
            firstMigration.Description.ShouldBe("This is my first migration");
            firstMigration.SqlStatements.Count().ShouldBe(1);
            firstMigration.SqlStatements.ShouldContain("blah!");

            var secondMigration = migrations.Single(m => m.TargetDatabaseVersion == 2);
            secondMigration.Description.ShouldBe("This is the next migration");
            secondMigration.SqlStatements.Count().ShouldBe(2);
            secondMigration.SqlStatements.ShouldContain("hello world!");
            secondMigration.SqlStatements.ShouldContain("another sql statement");
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

go

another sql statement"; }
        }
    }
}