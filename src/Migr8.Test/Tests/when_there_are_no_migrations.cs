using NUnit.Framework;
using Shouldly;

namespace Migr8.Test.Tests
{
    [TestFixture]
    public class when_there_are_no_migrations : DbFixtureFor<DatabaseMigrator>
    {
        protected override DatabaseMigrator Create()
        {
            return new DatabaseMigrator(TestDbConnectionString);
        }

        [Test]
        public void migrator_does_nothing()
        {
            // arrange
            

            // act
            sut.MigrateDatabase();

            // assert
            TestDb(db => db.TableNames().Count.ShouldBe(0));
        }
    }
}
