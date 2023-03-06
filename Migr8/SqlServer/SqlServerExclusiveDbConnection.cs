using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Migr8.Internals;

namespace Migr8.SqlServer
{
    class SqlServerExclusiveDbConnection : IExclusiveDbConnection
    {
        readonly Options _options;
        readonly SqlConnection _connection;
        readonly SqlTransaction _transaction;

        public SqlServerExclusiveDbConnection(string connectionString, Options options, bool useTransaction, bool useManagedIdentity, string tokenUrl)
        {
            _options = options;
            _connection = new SqlConnection(connectionString);

            if (useManagedIdentity)
            {
                _connection.AccessToken = AsyncHelpers.GetSync(async () =>
                {
                    try
                    {
                        return await new AzureServiceTokenProvider().GetAccessTokenAsync(tokenUrl);
                    }
                    catch (Exception exception)
                    {
                        throw new ApplicationException($"Could not get access token from '{tokenUrl}'", exception);
                    }
                });
            }

            _connection.Open();
            _transaction = useTransaction ? _connection.BeginTransaction(IsolationLevel.Serializable) : null;
        }

        public void Complete()
        {
            _transaction?.Commit();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _connection.Dispose();
        }

        public HashSet<string> GetTableNames()
        {
            var tableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using var command = CreateCommand();
            command.CommandText = "SELECT [TABLE_NAME] FROM [information_schema].[tables]";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                tableNames.Add((string)reader["TABLE_NAME"]);
            }

            return tableNames;
        }

        public void LogMigration(IExecutableSqlMigration migration, string migrationTableName)
        {
            using var command = CreateCommand();
            command.CommandText = $@"
INSERT INTO [{migrationTableName}] (
    [MigrationId],
    [Sql],
    [Description],
    [Time],
    [UserName],
    [UserDomainName],
    [MachineName]
) VALUES (
    @id,
    @sql,
    @description,
    @time,
    @userName,
    @userDomainName,
    @machineName
)
";

            command.Parameters.Add("id", SqlDbType.NVarChar, 200).Value = migration.Id;
            command.Parameters.Add("sql", SqlDbType.NVarChar).Value = migration.Sql;
            command.Parameters.Add("description", SqlDbType.NVarChar).Value = migration.Description;
            command.Parameters.Add("time", SqlDbType.DateTimeOffset).Value = DateTimeOffset.Now;
            command.Parameters.Add("userName", SqlDbType.NVarChar).Value = Environment.GetEnvironmentVariable("USERNAME") ?? "??";
            command.Parameters.Add("userDomainName", SqlDbType.NVarChar).Value = Environment.GetEnvironmentVariable("USERDOMAIN") ?? "??";
            command.Parameters.Add("machineName", SqlDbType.NVarChar).Value = Environment.MachineName;

            command.ExecuteNonQuery();
        }

        public void CreateMigrationTable(string migrationTableName)
        {
            using (var command = CreateCommand())
            {
                command.CommandText =
                    $@"
CREATE TABLE [{migrationTableName}] (
    [Id] INT IDENTITY(1,1),
    [MigrationId] NVARCHAR(200) NOT NULL,
    [Sql] NVARCHAR(MAX) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    [Time] DATETIMEOFFSET(3) NOT NULL,
    [UserName] NVARCHAR(MAX) NOT NULL,
    [UserDomainName] NVARCHAR(MAX) NOT NULL,
    [MachineName] NVARCHAR(MAX) NOT NULL,

    CONSTRAINT [PK_{migrationTableName}_Id] PRIMARY KEY ([Id])
);
";

                command.ExecuteNonQuery();
            }

            using (var command = CreateCommand())
            {
                command.CommandText =
                    $@"
ALTER TABLE [{migrationTableName}] 
    ADD CONSTRAINT [UNIQUE_{migrationTableName}_MigrationId] UNIQUE ([MigrationId]);
";

                command.ExecuteNonQuery();
            }
        }

        public void ExecuteStatement(string sqlStatement, TimeSpan? sqlCommandTimeout = null)
        {
            using var command = CreateCommand(sqlCommandTimeout);
            command.CommandText = sqlStatement;
            command.ExecuteNonQuery();
        }

        public IEnumerable<string> GetExecutedMigrationIds(string migrationTableName)
        {
            var list = new List<string>();

            using var command = CreateCommand();
            command.CommandText = $"SELECT [MigrationId] FROM [{migrationTableName}]";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add((string)reader["MigrationId"]);
            }

            return list;
        }

        SqlCommand CreateCommand(TimeSpan? sqlCommandTimeout = null)
        {
            var sqlCommand = _connection.CreateCommand();
            sqlCommand.Transaction = _transaction;
            sqlCommand.CommandTimeout = (int)(sqlCommandTimeout?.TotalSeconds ?? _options.SqlCommandTimeout.TotalSeconds);
            return sqlCommand;
        }
    }
}