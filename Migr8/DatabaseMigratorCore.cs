using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Migr8
{
    class DatabaseMigratorCore
    {
        public const string DefaultMigrationTableName = "__Migr8";

        readonly IWriter _writer;
        readonly string _connectionString;
        readonly string _migrationTableName;

        public DatabaseMigratorCore(IWriter writer, string connectionString, string migrationTableName = null)
        {
            _writer = writer;
            _connectionString = connectionString;
            _migrationTableName = migrationTableName ?? DefaultMigrationTableName;
        }

        public void Execute(IEnumerable<IExecutableSqlMigration> migrations)
        {
            var executableSqlMigrations = migrations.ToList();

            while (TryDoWork(executableSqlMigrations)) ;
        }

        bool TryDoWork(List<IExecutableSqlMigration> migrations)
        {
            using (var connection = new ExclusiveDbConnection(_connectionString))
            {
                EnsureMigrationTableExists(connection);

                var nextMigration = GetNextMigration(connection, migrations);

                if (nextMigration == null)
                {
                    return false;
                }

                ExecuteMigration(nextMigration, connection);

                connection.Complete();

                return true;
            }
        }

        void ExecuteMigration(IExecutableSqlMigration migration, ExclusiveDbConnection connection)
        {
            var id = migration.Id;

            _writer.Write($"Executing migration {id}");

            using (var command = connection.CreateCommand())
            {
                command.CommandText = migration.Sql;

                command.ExecuteNonQuery();
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = $@"
INSERT INTO [{_migrationTableName}] (
    [Id]
) VALUES (
    @id
)
";
                command.Parameters.Add("Id", SqlDbType.NVarChar, 200).Value = id;
                command.ExecuteNonQuery();
            }
        }

        IExecutableSqlMigration GetNextMigration(ExclusiveDbConnection connection, List<IExecutableSqlMigration> migrations)
        {
            var executedMigrationIds = connection.Select<string>("Id", $"SELECT [Id] FROM [{_migrationTableName}]");

            var remainingMigrations = migrations
                .Where(m => !executedMigrationIds.Contains(m.Id))
                .ToList();

            remainingMigrations.Sort((m1,m2) => m1.Id.CompareTo(m2.Id));

            return remainingMigrations.FirstOrDefault();
        }

        void EnsureMigrationTableExists(ExclusiveDbConnection connection)
        {
            var tableNames = connection.GetTableNames();

            if (!tableNames.Contains(_migrationTableName))
            {
                CreateMigrationTable(_migrationTableName, connection);
            }
        }

        void CreateMigrationTable(string migrationTableName, ExclusiveDbConnection connection)
        {
            _writer.Write($"Creating migration table '{migrationTableName}'");

            using (var command = connection.CreateCommand())
            {
                command.CommandText = $@"

CREATE TABLE [{migrationTableName}] (
    [Id] NVARCHAR(200) NOT NULL,
    
    CONSTRAINT [PK_{migrationTableName}_Id] PRIMARY KEY ([Id])
);

";

                command.ExecuteNonQuery();
            }
        }

        static SqlCommand CreateCommand(SqlConnection connection, SqlTransaction transaction)
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            return command;
        }
    }
}