using Migr8.Test.Assumptions;
using NUnit.Framework;
using Testy.General;
// ReSharper disable ClassNeverInstantiated.Local

namespace Migr8.Test.Bugs;

[TestFixture]
public class MigrationWithoutComment2 : DbFixtureBase
{
    protected override void SetUp()
    {
        base.SetUp();

        Using(new DisposableCallback(ResetDatabase));
    }

    [Test]
    public void ExecutesWithoutError()
    {
        var migrations = Migrations.FromAssemblyOf<CanReplaceDateTimeColumnWithDateTimeOffsetColumnNoProblem>()
            .Where(m => m.SqlMigration is MyMigrationWithoutDescription);

        Database.Migrate(TestConfig.ConnectionString, migrations);
    }

    [Migration(1)]
    class MyMigrationWithoutDescription : ISqlMigration
    {
        public string Sql => @"CREATE TABLE Bim (Id int not null)";
    }
}