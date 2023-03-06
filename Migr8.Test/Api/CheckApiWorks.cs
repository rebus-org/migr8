using System;
using System.Linq;
using NUnit.Framework;

namespace Migr8.Test.Api
{
    [TestFixture]
    public class CheckApiWorks : FixtureBase
    {
        [Test]
        public void ItWorks()
        {
            var migrations = Migrations.FromAssemblyOf<CheckApiWorks>()
                .Where(m => MyMigrations.All.Contains(m.SqlMigration.GetType()));

            Database.Migrate(TestConfig.ConnectionString, migrations, new Options(logAction: text => Console.WriteLine("LOG: {0}", text)));

            var tableNames = GetTableNames();

            Assert.That(tableNames, Is.EqualTo(new[] {"MyFirstTable", "MySecondTable"}));
        }
    }
}