using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Migr8.Internals;
using Migr8.Test.Basic;
using NUnit.Framework;

namespace Migr8.Test.Ordering
{
    [TestFixture]
    public class CheckSequence : DbFixtureBase
    {
        DatabaseMigratorCore _migrator;

        protected override void SetUp()
        {
            base.SetUp();
            _migrator = new DatabaseMigratorCore(new ThreadPrintingConsoleWriter(), TestConfig.ConnectionString);
        }

        static readonly TestMigration[] AllMigrations = {
            new TestMigration(0, "master", "CREATE TABLE [Table] ([Id] INT IDENTITY(1,1), [Number] INT NOT NULL)"),
            new TestMigration(1, "master", "INSERT INTO [Table] ([Number]) VALUES ('1')"),
            new TestMigration(2, "master", "INSERT INTO [Table] ([Number]) VALUES ('2')"),
            new TestMigration(3, "master", "INSERT INTO [Table] ([Number]) VALUES ('3')"),
            new TestMigration(4, "master", "INSERT INTO [Table] ([Number]) VALUES ('4')"),
            new TestMigration(5, "master", "INSERT INTO [Table] ([Number]) VALUES ('5')"),
            new TestMigration(6, "master", "INSERT INTO [Table] ([Number]) VALUES ('6')"),
            new TestMigration(7, "master", "INSERT INTO [Table] ([Number]) VALUES ('7')"),
            new TestMigration(8, "master", "INSERT INTO [Table] ([Number]) VALUES ('8')"),
            new TestMigration(9, "master", "INSERT INTO [Table] ([Number]) VALUES ('9')"),
            new TestMigration(10, "master", "INSERT INTO [Table] ([Number]) VALUES ('10')"),
            new TestMigration(11, "master", "INSERT INTO [Table] ([Number]) VALUES ('11')"),

        };

        [Test]
        public void DoesItInTheRightOrder()
        {
            _migrator.Execute(AllMigrations);

            using var connection = new SqlConnection(TestConfig.ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT [Number] FROM [Table] ORDER BY [Id]";

            var actualNumbers = new List<int>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    actualNumbers.Add((int)reader["Number"]);
                }
            }
            //var actualNumbers = connection
            //    .Select<int>("Number", "SELECT [Number] FROM [Table] ORDER BY [Id]")
            //    .ToArray();

            var expectedNumbers = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11};

            Assert.That(actualNumbers, Is.EqualTo(expectedNumbers),
                $"Expected {string.Join(", ", expectedNumbers)} but got {string.Join(", ", actualNumbers)}");
        }
    }
}