using NUnit.Framework;
using System;
using System.IO;

namespace Migr8.Mysql.Test.hints
{
    [TestFixture]
    public class TestSqlCommandTimeoutHint : MysqlFixtureBase
    {
        [Test]
        public void ExecuteMigrationWithTimeout()
        {
            var migrations = Migrations.FromFilesIn(Path.Combine(AppContext.BaseDirectory, "hints", "scripts"));

            var options = new Options(sqlCommandTimeout: TimeSpan.FromSeconds(10));

            Database.Migrate(TestConfig.MysqlConnectionString, migrations, options);

        }

        [Test]
        public void ExecuteMigrationsFromCode()
        {
            var migrations = Migrations.FromAssemblyOf<TestSqlCommandTimeoutHint>();

            var options = new Options(sqlCommandTimeout: TimeSpan.FromSeconds(10));

            Database.Migrate(TestConfig.MysqlConnectionString, migrations, options);

        }
        
        [Migration(4,"Adds card table")]
        [Hint(Hints.SqlCommandTimeout, "00:00:30")]
        public class CreateCardsTable : ISqlMigration
        {
            public string Sql => @"CREATE TABLE `Cards`( ID INT AUTO_INCREMENT, Name TEXT, Description TEXT, PRIMARY KEY (ID)); DO SLEEP(20)";
        }
    }
}
