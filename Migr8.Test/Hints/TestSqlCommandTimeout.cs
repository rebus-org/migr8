using System;
using System.IO;
using NUnit.Framework;

namespace Migr8.Test.Hints;

[TestFixture]
public class TestSqlCommandTimeout : DbFixtureBase
{
    [Test]
    public void CanSetSqlCommandTimeoutViaHint()
    {
        var migrations = Migrations.FromFilesIn(Path.Combine(AppContext.BaseDirectory, "Hints", "Migrations"));

        Database.Migrate(TestConfig.ConnectionString, migrations, options: new(sqlCommandTimeout: TimeSpan.FromSeconds(2)));
    }
}