using FakeItEasy;
using Migr8.Internal;
using NUnit.Framework;
using Shouldly;

namespace Migr8.Test.DatabaseMigrator
{
    public class when_sequence_is_broken : DbFixtureFor<Migr8.DatabaseMigrator>
    {
        IProvideMigrations provideMigrations;

        protected override Migr8.DatabaseMigrator Create()
        {
            provideMigrations = A.Fake<IProvideMigrations>();

            A.CallTo(() => provideMigrations.GetAllMigrations())
                .Returns(new[]
                             {
                                 NewMigration(1, "--"),
                                 NewMigration(2, "--"),
                                 NewMigration(4, "--"),
                                 NewMigration(5, "--"),
                             });

            return new Migr8.DatabaseMigrator(TestDbConnectionString, provideMigrations);
        }

        [Test]
        public void an_exception_is_thrown()
        {
            // act
            var exception = Should.Throw<DatabaseMigrationException>(() => sut.MigrateDatabase());

            // assert
            exception.ToString().ToLowerInvariant().ShouldContain("missing");
            exception.ToString().ToLowerInvariant().ShouldContain("version 3");
        }

        IMigration NewMigration(int targetDatabaseVersion, string sql)
        {
            var migration = A.Fake<IMigration>();
            A.CallTo(() => migration.TargetDatabaseVersion).Returns(targetDatabaseVersion);
            A.CallTo(() => migration.SqlStatements).Returns(new[] { sql });
            return migration;
        }
    }
}