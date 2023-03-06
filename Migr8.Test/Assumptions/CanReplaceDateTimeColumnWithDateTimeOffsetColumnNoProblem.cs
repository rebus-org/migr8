using System;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Testy.General;

namespace Migr8.Test.Assumptions;

[TestFixture]
public class CanReplaceDateTimeColumnWithDateTimeOffsetColumnNoProblem : FixtureBase
{
    protected override void SetUp()
    {
        base.SetUp();

        //Using(new DisposableCallback(ResetDatabase));
    }

    [Test]
    public async Task SimulateMigrationScenario()
    {
        await CreateMigrationsTableWithOldLayout();

        await InsertMigrationRowToEmulateOneMigrationHavingBeenExecuted();

        var migrations = Migrations
            .FromAssemblyOf<CanReplaceDateTimeColumnWithDateTimeOffsetColumnNoProblem>()
            .Where(m => m.SqlMigration is MyNextMigration);

        Database.Migrate(TestConfig.ConnectionString, migrations);
    }

    static async Task CreateMigrationsTableWithOldLayout()
    {
        var tableName = Options.DefaultMigrationTableName;

        await ExecuteSql($@"

CREATE TABLE [{tableName}] (
    [Id] INT IDENTITY(1,1),
    [MigrationId] NVARCHAR(200) NOT NULL,
    [Sql] NVARCHAR(MAX) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    [Time] DATETIME2 NOT NULL,
    [UserName] NVARCHAR(MAX) NOT NULL,
    [UserDomainName] NVARCHAR(MAX) NOT NULL,
    [MachineName] NVARCHAR(MAX) NOT NULL,

    CONSTRAINT [PK_{tableName}_Id] PRIMARY KEY ([Id])
);

");
    }

    static async Task InsertMigrationRowToEmulateOneMigrationHavingBeenExecuted()
    {
        // Id	MigrationId	Sql	Description	Time	UserName	UserDomainName	MachineName
        // 1   1 - master    CREATE TABLE Bim(Id int not null)      2023 - 03 - 06 13:50:33.5168167 extgrabemo INTAD1  LTPF2BVYBC
        var tableName = Options.DefaultMigrationTableName;

        await ExecuteSql($@"INSERT INTO [{tableName}] VALUES (1, '1-master', 'fake migration', '{DateTime.Now:yyyy-MM-dd}', 'username', 'domain', 'machine')");
    }

    private static async Task ExecuteSql(string sql)
    {
        await using var connection = new SqlConnection(TestConfig.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception exception)
        {
            throw new AssertionException($@"Error when executing SQL

{sql}", exception);
        }
    }

    [Migration(2)]
    class MyNextMigration : ISqlMigration
    {
        public string Sql => @"CREATE TABLE Bim (Id int not null)";
    }
}