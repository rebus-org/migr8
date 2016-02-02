using NUnit.Framework;

namespace Migr8.Npgsql.Test
{
    [TestFixture]
    public class PostgresIntegrationTest : PostgresFixtureBase
    {
        [Test]
        public void Run()
        {
            Database.Migrate(TestConfig.PostgresConnectionString, GetMigrations());
        }

        static Migrations GetMigrations()
        {
            return new Migrations(new[]
            {
                new TestMigration(1, "master", ""), 
                new TestMigration(2, "master", ""), 
            });
        }
    }
}