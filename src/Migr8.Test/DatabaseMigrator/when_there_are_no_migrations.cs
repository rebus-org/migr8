using FakeItEasy;
using Migr8.DB;
using Migr8.Internal;
using NUnit.Framework;
using Shouldly;

namespace Migr8.Test.DatabaseMigrator
{
    [TestFixture]
    public class when_there_are_no_migrations : DbFixtureFor<Migr8.DatabaseMigrator>
    {
        protected override Migr8.DatabaseMigrator Create()
        {
            return new Migr8.DatabaseMigrator(TestDbConnectionString, A.Fake<IProvideMigrations>());
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
