using NUnit.Framework;

namespace Migr8.Test.Api
{
    [TestFixture]
    public class CheckFiltering : FixtureBase
    {
        [Test]
        public void ItWorks()
        {
            Database.Migrate(TestConfig.ConnectionString, Migrations.FromThisAssembly().Where(m => m.SequenceNumber < 2));

            var tableNames = GetTableNames();

            Assert.That(tableNames, Is.EqualTo(new[] {"MyFirstTable"}));
        }
    }
}