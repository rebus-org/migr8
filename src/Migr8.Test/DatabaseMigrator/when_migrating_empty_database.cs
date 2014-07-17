using System.Globalization;
using System.Linq;
using FakeItEasy;
using Migr8.DB;
using Migr8.Internal;
using NUnit.Framework;
using Shouldly;

namespace Migr8.Test.DatabaseMigrator
{
    [TestFixture]
    public class when_migrating_empty_database : DbFixtureFor<Migr8.DatabaseMigrator>
    {
        private string _defaultVersionTableName = "DBVersion";

        protected override Migr8.DatabaseMigrator Create()
        {
            return new Migr8.DatabaseMigrator(TestDbConnectionString, A.Fake<IProvideMigrations>(), new Options());
        }

        protected Migr8.DatabaseMigrator Create(Options options)
        {
            return new Migr8.DatabaseMigrator(TestDbConnectionString, A.Fake<IProvideMigrations>(), options);
        }

        [TestCase(0)]
        [TestCase(1)]
        public void database_version_is_added_as_database_metadata(int persisterType)
        {
            // arrange
            var options = new Options();
            switch (persisterType)
            {
                case 1:
                    options.UseVersionTableName(_defaultVersionTableName);
                    break;
                case 0:
                    break;
            }

            sut = Create(options);

            // act
            sut.MigrateDatabase();

            // assert
            if (persisterType == 0)
            {
                TestDb(db => db.DatabaseProperties().ShouldContainKey(Constants.DatabaseVersionPropertyName));
            }
            else if (persisterType == 1)
            {
                TestDb(db =>
                {
                    db.TableNames().Count().ShouldBe(1);
                    db.TableNames().Single().ShouldBe(_defaultVersionTableName);
                });
            }
        }

        [TestCase(0)]
        [TestCase(1)]
        public void database_version_starts_with_0(int persisterType)
        {
            // arrange
            var options = new Options();
            switch (persisterType)
            {
                case 1:
                    options.UseVersionTableName(_defaultVersionTableName);
                    break;
                case 0:
                    break;
            }
            sut = Create(options);

            // act
            sut.MigrateDatabase();

            // assert
            if (persisterType == 0)
            {
                TestDb(db => db.DatabaseProperties()[Constants.DatabaseVersionPropertyName].ShouldBe(0.ToString(CultureInfo.InvariantCulture)));
            }
            else if (persisterType == 1)
            {
                TestDb(db => db.GetSingleValue<int>(_defaultVersionTableName, "MigrationVersion").ShouldBe(0));
            }
        }
    }
}