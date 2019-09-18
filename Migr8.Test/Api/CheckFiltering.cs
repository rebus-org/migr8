using System;
using NUnit.Framework;
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther

namespace Migr8.Test.Api
{
    [TestFixture]
    public class CheckFiltering : FixtureBase
    {
        [Test]
        public void ItWorks()
        {
            Database.Migrate(
                connectionString: TestConfig.ConnectionString,
                migrations: Migrations.FromAssemblyOf<CheckFiltering>().Where(m => m.SequenceNumber < 2),
                options: new Options(sqlCommandTimeout: TimeSpan.FromMinutes(20))
            );

            var tableNames = GetTableNames();

            Assert.That(tableNames, Is.EqualTo(new[] { "MyFirstTable" }));
        }
    }
}