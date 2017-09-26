using NUnit.Framework;

namespace Migr8.Mysql.Test
{
    [TestFixture]
    public class MysqlIntegrationTest : MysqlFixtureBase
    {
        [Test]
        public void Run()
        {
            Database.Migrate(TestConfig.MysqlConnectionString, GetMigrations(), new Options(logAction: Log, verboseLogAction: LogVerbose));
        }

        static Migrations GetMigrations()
        {
            return new Migrations(new[]
            {
                new TestMigration(1, "master", @"
CREATE TABLE `bimmelim` (
    `Id` BIGINT NOT NULL,
	`text` TEXT,
	PRIMARY KEY (`Id`)
);
"), 
                new TestMigration(2, "master", @"INSERT INTO `bimmelim` (`text`) VALUES ('HEJ DU');"),
                new TestMigration(3, "master", @"DROP TABLE `bimmelim`;")
            });
            
        }
    }
}