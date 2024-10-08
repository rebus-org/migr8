﻿using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Testy.Files;

namespace Migr8.Test.Bugs
{
    [TestFixture]
    public class MigrationWithoutComment : DbFixtureBase
    {
        [TestCase(@"-- wat

CREATE TABLE [TestTable] ([Id] INT)", "wat", "CREATE TABLE [TestTable] ([Id] INT)")]
        [TestCase(@"-- wat
--wat2
--  wat3

CREATE TABLE [TestTable] ([Id] INT)", @"wat
wat2
wat3", "CREATE TABLE [TestTable] ([Id] INT)")]

        [TestCase(@"-- wat
CREATE TABLE [TestTable] ([Id] INT)", "wat", "CREATE TABLE [TestTable] ([Id] INT)")]

        [TestCase(@"CREATE TABLE [TestTable] ([Id] INT)", "", "CREATE TABLE [TestTable] ([Id] INT)")]
        public void PicksUpMigrationSqlAsExpected(string contents, string expectedDescription, string expectedSql)
        {
            var directory = Using(new TemporaryTestDirectory());

            File.WriteAllText(Path.Combine(directory, "001-master.sql"), contents);

            var migrations = Migrations.FromFilesIn(directory).ToList();

            Assert.That(migrations.Count, Is.EqualTo(1));

            var migration = migrations.First();

            Assert.That(NormalizeLineBreaks(migration.Description), Is.EqualTo(NormalizeLineBreaks(expectedDescription)));
            Assert.That(migration.SqlMigration.Sql, Is.EqualTo(expectedSql));
        }

        private static string NormalizeLineBreaks(string str) => string.Join(Environment.NewLine, str.Split(new[] { Environment.NewLine, "\r", "\n" }, StringSplitOptions.None));
    }
}