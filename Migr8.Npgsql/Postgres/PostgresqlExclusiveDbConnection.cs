using System;
using System.Collections.Generic;
using System.Data;
using Migr8.Internals;
using Npgsql;
using NpgsqlTypes;

namespace Migr8.Postgres
{
    class PostgresqlExclusiveDbConnection : IExclusiveDbConnection
    {
        readonly NpgsqlConnection _connection;
        readonly NpgsqlTransaction _transaction;

        public PostgresqlExclusiveDbConnection(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction(IsolationLevel.Serializable);
        }

        public void Dispose()
        {
            _transaction.Dispose();
            _connection.Dispose();
        }

        public void Complete()
        {
            _transaction.Commit();
        }

        public HashSet<string> GetTableNames()
        {
            var tableNames = new HashSet<string>();

            using (var command = CreateCommand())
            {
                command.CommandText = @"SELECT * FROM ""information_schema"".""tables"" WHERE ""table_schema"" NOT IN ('pg_catalog', 'information_schema')";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tableNames.Add(reader["table_name"].ToString());
                    }
                }
            }

            return tableNames;
        }

        public void LogMigration(IExecutableSqlMigration migration, string migrationTableName)
        {
            using (var command = CreateCommand())
            {
                command.CommandText = $@"
INSERT INTO ""{migrationTableName}"" (
    ""MigrationId"",
    ""Sql"",
    ""Description"",
    ""Time"",
    ""UserName"",
    ""UserDomainName"",
    ""MachineName""
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

                command.Parameters.Add("id", NpgsqlDbType.Text).Value = migration.Id;
                command.Parameters.Add("sql", NpgsqlDbType.Text).Value = migration.Sql;
                command.Parameters.Add("description", NpgsqlDbType.Text).Value = migration.Description;
                command.Parameters.Add("time", NpgsqlDbType.TimestampTZ).Value = DateTime.Now;
                command.Parameters.Add("userName", NpgsqlDbType.Text).Value = Environment.UserName;
                command.Parameters.Add("userDomainName", NpgsqlDbType.Text).Value = Environment.UserDomainName;
                command.Parameters.Add("machineName", NpgsqlDbType.Text).Value = Environment.MachineName;

                command.ExecuteNonQuery();
            }
        }

        public void CreateMigrationTable(string migrationTableName)
        {
            using (var command = CreateCommand())
            {
                command.CommandText =
                    $@"
CREATE TABLE ""{migrationTableName}"" (
    ""Id"" BIGSERIAL PRIMARY KEY,
    ""MigrationId"" TEXT NOT NULL,
    ""Sql"" TEXT NOT NULL,
    ""Description"" TEXT NOT NULL,
    ""Time"" TIMESTAMP WITH TIME ZONE NOT NULL,
    ""UserName"" TEXT NOT NULL,
    ""UserDomainName"" TEXT NOT NULL,
    ""MachineName"" TEXT NOT NULL
);
";

                command.ExecuteNonQuery();
            }

            using (var command = CreateCommand())
            {
                command.CommandText =
                    $@"
ALTER TABLE ""{migrationTableName}""
    ADD CONSTRAINT ""UNIQUE_{migrationTableName}_MigrationId"" UNIQUE (""MigrationId"");
";

                command.ExecuteNonQuery();
            }
        }

        public void ExecuteStatement(string sqlStatement)
        {
            using (var command = CreateCommand())
            {
                command.CommandText = sqlStatement;
                command.ExecuteNonQuery();
            }
        }

        public IEnumerable<string> GetExecutedMigrationIds(string migrationTableName)
        {
            var list = new List<string>();
            using (var command = CreateCommand())
            {
                command.CommandText = $@"SELECT ""MigrationId"" FROM ""{migrationTableName}""";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add((string)reader["MigrationId"]);
                    }
                }
            }
            return list;

        }

        NpgsqlCommand CreateCommand()
        {
            var sqlCommand = _connection.CreateCommand();
            sqlCommand.Transaction = _transaction;
            return sqlCommand;
        }
    }
}