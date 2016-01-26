using Migr8.Test.Basic;
using NUnit.Framework;

namespace Migr8.Test.Postgres
{
    [TestFixture]
    public class PostgresIntegrationTest : PostgresFixtureBase
    {
        [Test]
        public void Run()
        {
            Database.Migrate(TestConfig.PostgresConnectionString, GetMigrations(), new Options(db: Db.PostgreSql));
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