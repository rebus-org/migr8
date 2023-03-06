using System;
using NUnit.Framework;
using Testy.Extensions;
// ReSharper disable ClassNeverInstantiated.Local
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
            var migrations = Migrations.FromAssemblyOf<CheckFiltering>()
                .Where(m => m.SqlMigration is CreateMyFirstTable);

            Console.WriteLine("Found migrations:");

            migrations.ToList().DumpTable();

            Database.Migrate(
                connectionString: TestConfig.ConnectionString,
                migrations: migrations,
                options: new Options(sqlCommandTimeout: TimeSpan.FromMinutes(20))
            );

            var tableNames = GetTableNames();

            Assert.That(tableNames, Is.EqualTo(new[] { "MyFirstTable" }));
        }

        [Migration(1, "This is my first migration", "master")]
        class CreateMyFirstTable : ISqlMigration
        {
            public string Sql => @"CREATE TABLE [MyFirstTable] ([Id] int)";
        }
    }
}