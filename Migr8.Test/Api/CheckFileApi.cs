using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Migr8.Test.Api
{
    [TestFixture]
    public class CheckFileApi : FixtureBase
    {
        [Test]
        public void CanPickUpMigrationsFromFiles()
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Api");

            Database.Migrate(TestConfig.ConnectionString, Migrations.FromFilesIn(dir));

            var tableNames = GetTableNames().ToList();

            Assert.That(tableNames, Is.EqualTo(new[] {"Tabelle1", "Tabelle2", "Tabelle3"}));
        }
    }
}