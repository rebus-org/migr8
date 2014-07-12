using FakeItEasy;
using Migr8.DB;
using Migr8.Internal;
using NUnit.Framework;
using Shouldly;

namespace Migr8.Test.DatabaseMigrator
{
    [TestFixture]
    public class when_migrating_empty_database : DbFixtureFor<Migr8.DatabaseMigrator>
    {
        protected override Migr8.DatabaseMigrator Create()
        {
            return new Migr8.DatabaseMigrator(TestDbConnectionString, A.Fake<IProvideMigrations>());
        }

        [Test]
        public void database_version_is_added_as_database_metadata()
        {
            // arrange

            // act
            sut.MigrateDatabase();

            // assert
            TestDb(db => db.DatabaseProperties().ShouldContainKey(Constants.DatabaseVersionPropertyName));
        }

        [Test]
        public void database_version_starts_with_0()
        {
            // arrange

            // act
            sut.MigrateDatabase();

            // assert
            TestDb(db => db.DatabaseProperties()[Constants.DatabaseVersionPropertyName].ShouldBe(0.ToString()));
        }
    }
}