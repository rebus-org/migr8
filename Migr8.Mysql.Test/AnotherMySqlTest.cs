using System.IO;
using NUnit.Framework;

namespace Migr8.Mysql.Test
{
    [TestFixture]
    public class AnotherMySqlTest : MysqlFixtureBase
    {
        [Test]
        public void TryWithFiles()
        {
            var directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "Scripts");
            Database.Migrate(TestConfig.MysqlConnectionString, Migrations.FromFilesIn(directory), new Options(logAction: Log, verboseLogAction: LogVerbose));
        }
    }
}