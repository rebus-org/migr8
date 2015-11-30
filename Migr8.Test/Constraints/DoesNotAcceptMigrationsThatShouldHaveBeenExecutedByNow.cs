using System;
using System.Collections.Generic;
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

        [Test]
        public void StillWorksWithParallelDevelopmentThough()
        {
            var firstBatch = new[]
            {
                new TestMigration(1, "test", Create("test1")),
                new TestMigration(2, "feature-A", Create("A2")),
                new TestMigration(3, "feature-A", Create("A3")),
                new TestMigration(4, "feature-A", Create("A4")),
                new TestMigration(5, "test", Create("test5")),
            };

            Database.Migrate(TestConfig.ConnectionString, new Migrations(firstBatch));

            var secondBatchAfterMerge = new[]
            {
                new TestMigration(1, "test", Create("test1")),

                new TestMigration(2, "feature-A", Create("A2")),
                new TestMigration(3, "feature-A", Create("A3")),
                new TestMigration(4, "feature-A", Create("A4")),

                new TestMigration(2, "feature-B", Create("B2")),
                new TestMigration(3, "feature-B", Create("B3")),
                new TestMigration(4, "feature-B", Create("B4")),

                new TestMigration(5, "test", Create("test5")),
            };

            var migrations = new Migrations(secondBatchAfterMerge);

            Database.Migrate(TestConfig.ConnectionString, migrations);

            var tableNames = GetTableNames();

            Assert.That(tableNames, Is.EqualTo(new[] {"A2", "A3", "A4", "B2", "B3", "B4", "test1", "test5"}));
        }

        static string Create(string tableName)
        {
            return $"CREATE TABLE [{tableName}] ([id] int)";
        }
    }
}