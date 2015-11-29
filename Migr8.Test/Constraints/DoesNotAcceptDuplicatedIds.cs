using System;
using Migr8.Test.Basic;
using NUnit.Framework;

namespace Migr8.Test.Constraints
{
    [TestFixture]
    public class DoesNotAcceptDuplicatedIds : FixtureBase
    {
        [Test]
        public void TryIt()
        {
            var migrations = new Migrations(new[]
            {
                new TestMigration(1, "master", ""),
                new TestMigration(2, "feature-a", ""),
                new TestMigration(2, "feature-b", ""),
                new TestMigration(3, "feature-c", ""),
                new TestMigration(3, "feature-c", ""),
                new TestMigration(4, "master", ""),
            });

            var ex = Assert.Throws<MigrationException>(() =>
            {
                Database.Migrate(TestConfig.ConnectionString, migrations);
            });

            Console.WriteLine(ex);

            Assert.That(ex.Message, Contains.Substring("feature-c"));
        }

    }
}