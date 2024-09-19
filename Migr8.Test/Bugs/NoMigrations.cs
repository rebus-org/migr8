using NUnit.Framework;

namespace Migr8.Test.Bugs
{
    [TestFixture]
    public class NoMigrations : DbFixtureBase
    {
        [Test]
        public void ReturnsJustFineEvenWhenThereAreNoMigrations()
        {
            Database.Migrate(TestConfig.ConnectionString, Migrations.None);
        }
    }
}