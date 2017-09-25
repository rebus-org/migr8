using NUnit.Framework;

namespace Migr8.Mysql.Test
{
    [TestFixture]
    public class AnotherMySqlTest : MysqlFixtureBase
    {
        [Test]
        public void TryWithFiles()
        {
            Database.Migrate(TestConfig.MysqlConnectionString, Migrations.FromFilesIn("Scripts"), new Options(logAction: Log, verboseLogAction: LogVerbose));
        }
    }
}