using NUnit.Framework;
using Shouldly;

namespace Migr8.Test.Tests
{
    [TestFixture]
    public class when_migrating_empty_database : DbFixtureFor<DatabaseMigrator>
    {
        protected override DatabaseMigrator SetUp()
        {
            return new DatabaseMigrator(ConnectionString, testDatabaseName);
        }

        [Test]
        public void database_version_is_added_as_datbase_metadata()
        {
            // arrange
            

            // act
            sut.MigrateDatabase();

            // assert
            TestDb(db => db.DatabaseProperties().ShouldContainKeyAndValue("migr8_database_version", 1.ToString()));
        }
    }
}