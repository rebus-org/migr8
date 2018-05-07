using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Migr8.Test.Api
{
    [TestFixture]
    public class CheckFileApi : FixtureBase
    {
        string _directory;

        protected override void SetUp()
        {
            _directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Api");
        }

        [Test]
        public void CanPickUpMigrationsFromFiles()
        {
            Database.Migrate(TestConfig.ConnectionString, Migrations.FromFilesIn(_directory));

            var tableNames = GetTableNames().ToList();

            Assert.That(tableNames, Is.EqualTo(new[] {"Tabelle1", "Tabelle2", "Tabelle3"}));
        }

        [Test]
        public void ExtractsHintsFromCommentsSection()
        {
            var migrations = Migrations.FromFilesIn(_directory).ToList()
                .OrderBy(m => m.SequenceNumber).ThenBy(m => m.BranchSpecification)
                .ToList();

            Assert.That(migrations.Select(m => $"{m.SequenceNumber}-{m.BranchSpecification}"), Is.EqualTo(new[]
            {
                "1-master",
                "2-feature-subdir1",
                "2-feature-subdir2",
            }));

            Assert.That(migrations.Select(m => string.Join(",", m.Hints)), Is.EqualTo(new[]
            {
                "hint1,hint2,hint3,hint4,hint5,hint-6",
                "",
                "no-transaction"
            }));
        }

        [Test]
        public void CorrectlyParsesSqlFiles()
        {
            var migrations = Migrations.FromFilesIn(_directory).ToList();

            Assert.That(migrations.Count, Is.EqualTo(3));

            var migrationWithInterestingComment = migrations.Single(m => m.SequenceNumber == 1 && m.BranchSpecification == "master");

            Console.WriteLine();
            Console.WriteLine("THIS IS THE DESCRIPTION:");
            Console.WriteLine(migrationWithInterestingComment.Description);
            Console.WriteLine();
            Console.WriteLine("THIS IS THE MIGRATION:");
            Console.WriteLine(migrationWithInterestingComment.SqlMigration.Sql);
            Console.WriteLine();

            Assert.That(migrationWithInterestingComment.Description, Is.EqualTo("This is my first migration{newline}A table is created{newline}{newline}This comment SHOULD be included, because it\'s part of the first comment block{newline}{newline}hints: hint1, hint2,    hint3;   hint4{newline}hints: hint5, hint-6".Replace("{newline}", Environment.NewLine)));

            Assert.That(migrationWithInterestingComment.SqlMigration.Sql, Is.EqualTo("-- This comment should NOT be included, because it\'s not considered connected to the first comment block{newline}-- Create a table{newline}create table [Tabelle1]{newline}({newline}\t[Id] int identity(1,1){newline}){newline}go{newline}-- Add a column to that table{newline}alter table [Tabelle1] add [Text] nvarchar(10);".Replace("{newline}", Environment.NewLine)));
        }
    }
}