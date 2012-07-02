using NUnit.Framework;
using Shouldly;

namespace Migr8.Test.Tests
{
    [TestFixture]
    public class when_migrating_empty_database : DbFixtureFor<DatabaseMigrator>
    {
        protected override DatabaseMigrator Create()
        {
            return new DatabaseMigrator(TestDbConnectionString);
        }

        [Test]
        public void database_version_is_added_as_database_metadata()
        {
            // arrange

            // act
            sut.MigrateDatabase();

            // assert
            TestDb(db => db.DatabaseProperties().ShouldContainKeyAndValue("migr8_database_version", 1.ToString()));
        }
    }
}