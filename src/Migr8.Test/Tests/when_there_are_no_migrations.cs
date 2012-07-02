using FakeItEasy;
using NUnit.Framework;
using Shouldly;

namespace Migr8.Test.Tests
{
    [TestFixture]
    public class when_there_are_no_migrations : DbFixtureFor<DatabaseMigrator>
    {
        protected override DatabaseMigrator Create()
        {
            return new DatabaseMigrator(TestDbConnectionString, A.Fake<IProvideMigrations>());
        }

        [Test]
        public void database_stays_empty()
        {
            // arrange
            

            // act
            sut.MigrateDatabase();

            // assert
            TestDb(db => db.TableNames().Count.ShouldBe(0));
        }
    }
}
