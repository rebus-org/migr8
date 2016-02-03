using NUnit.Framework;

namespace Migr8.Npgsql.Test
{
    [TestFixture]
    public class AnotherPostgresTest : PostgresFixtureBase
    {
        [Test]
        public void TryWithFiles()
        {
            Database.Migrate(TestConfig.PostgresConnectionString, Migrations.FromFilesIn("Scripts"), new Options(logAction: Log, verboseLogAction: LogVerbose));
        }
    }
}