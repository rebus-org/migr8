using System;
using Migr8.Test.Basic;
using NUnit.Framework;

namespace Migr8.Test.Constraints
{
    [TestFixture]
    public class DoesNotAcceptMigrationsThatShouldHaveBeenExecutedByNow : FixtureBase
    {
        [Test]
        public void TryIt()
        {
            var firstBatch = new[] {new TestMigration(1, "test", "")};

            Database.Migrate(TestConfig.ConnectionString, new Migrations(firstBatch));

            var secondBatchWhereSomeoneSwoopedInANewMigration = new[]
            {
                new TestMigration(0, "test", ""),
                new TestMigration(1, "test", "")
            };

            var ex = Assert.Throws<MigrationException>(() =>
            {
                var migrations = new Migrations(secondBatchWhereSomeoneSwoopedInANewMigration);

                Database.Migrate(TestConfig.ConnectionString, migrations);
            });

            Console.WriteLine(ex);

            Assert.That(ex.Message, Contains.Substring("0-test"));
        }
    }
}