using System.Linq;
using Migr8.Internals;
using NUnit.Framework;

namespace Migr8.Test.Bugs
{
    [TestFixture]
    public class NoMigrations : FixtureBase
    {
        [Test]
        public void ReturnsJustFineEvenWhenThereAreNoMigrations()
        {
            Database.Migrate(TestConfig.ConnectionString, Migrations.None);
        }
    }
}