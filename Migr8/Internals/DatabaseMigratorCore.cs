using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace Migr8.Internals
{
    class DatabaseMigratorCore
    {
        readonly IWriter _writer;
        readonly string _connectionString;
        readonly string _migrationTableName;

        public DatabaseMigratorCore(IWriter writer, string connectionString, string migrationTableName = null)
        {
            _writer = writer;
            _connectionString = connectionString;
            _migrationTableName = migrationTableName ?? Options.DefaultMigrationTableName;
        }

        public void Execute(IEnumerable<IExecutableSqlMigration> migrations)
        {
            var executableSqlMigrations = migrations.ToList();

            if (executableSqlMigrations.Count == 0)
            {
                _writer.Write("Found no migrations");
                return;
            }

            _writer.Write($"Migr8 found {executableSqlMigrations.Count} migrations");

            var duplicatedMigrations = executableSqlMigrations
                .GroupBy(m => m.Id)
                .Where(g => g.Count() > 1)
                .ToList();

            if (duplicatedMigrations.Any())
            {
                var duplicatedIds = string.Join(", ", duplicatedMigrations.SelectMany(g => g.Select(m => m.Id)));

                throw new MigrationException($"Cannot execute migrations because the following migrations are duplicates: {duplicatedIds}");
            }

            while (true)
            {
                var didExecuteMigration = TryDoWork(executableSqlMigrations);

                if (!didExecuteMigration)
                {
                    _writer.Write("No more migrations to run");
                    break;
                }
            }
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

                try
                {
                    ExecuteMigration(nextMigration, connection);

                    connection.Complete();

                    return true;
                }
                catch (Exception exception)
                {
                    throw new MigrationException($"Could not execute migration with ID '{nextMigration.Id}': {nextMigration.Sql}", exception);
                }
            }
        }

        void ExecuteMigration(IExecutableSqlMigration migration, ExclusiveDbConnection connection)
        {
            LogMigration(connection, migration);

            var id = migration.Id;
            var sql = migration.Sql;

            const RegexOptions options = RegexOptions.Multiline
                                         | RegexOptions.IgnorePatternWhitespace
                                         | RegexOptions.IgnoreCase;

            const string searchPattern = @"^\s*GO\s* ($ | \-\- .*$)";

            var sqlStatements = Regex.Split(sql, searchPattern, options)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim(' ', '\r', '\n'));

            foreach (var sqlStatement in sqlStatements)
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sqlStatement;
                    command.ExecuteNonQuery();
                }
            }

            _writer.Write($"Migration {id} executed");
        }

        IExecutableSqlMigration GetNextMigration(ExclusiveDbConnection connection, List<IExecutableSqlMigration> migrations)
        {
            var executedMigrationIds = connection
                .Select<string>("MigrationId", $"SELECT [MigrationId] FROM [{_migrationTableName}]")
                .ToList();

            executedMigrationIds.Sort(CompareMigrationId);

            var remainingMigrations = migrations
                .Where(m => !executedMigrationIds.Contains(m.Id))
                .ToList();

            remainingMigrations.Sort((m1, m2) => CompareMigrationId(m1.Id, m2.Id));

            var nextMigration = remainingMigrations.FirstOrDefault();

            if (executedMigrationIds.Any() && nextMigration != null)
            {
                var id1 = executedMigrationIds.First();
                var id2 = nextMigration.Id;

                if (CompareMigrationId(id1, id2) > 0)
                {
                    throw new MigrationException($"Cannot execute migrations because migration {id2} should have been executed before {id1}, which has already been executed!");
                }
            }

            return nextMigration;
        }

        static int CompareMigrationId(string id1, string id2)
        {
            var id1Tokens = id1.Split('-');
            var id2Tokens = id2.Split('-');

            var firstPart1 = id1Tokens.First();
            var firstPart2 = id2Tokens.First();

            var firstPartComparison = CompareFirstPart(firstPart1, firstPart2);

            if (firstPartComparison != 0)
            {
                return firstPartComparison;
            }

            var lastPart1 = string.Join("-", id1Tokens.Skip(1));
            var lastPart2 = string.Join("-", id2Tokens.Skip(1));

            return string.Compare(lastPart1, lastPart2, StringComparison.InvariantCultureIgnoreCase);
        }

        static int CompareFirstPart(string firstPart1, string firstPart2)
        {
            var number1 = int.Parse(firstPart1);
            var number2 = int.Parse(firstPart2);
            return number1.CompareTo(number2);
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
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $@"

CREATE TABLE [{migrationTableName}] (
    [Id] INT IDENTITY(1,1),
    [MigrationId] NVARCHAR(200) NOT NULL,
    [Sql] NVARCHAR(MAX) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    [Time] DATETIME2 NOT NULL,
    [UserName] NVARCHAR(MAX) NOT NULL,
    [UserDomainName] NVARCHAR(MAX) NOT NULL,
    [MachineName] NVARCHAR(MAX) NOT NULL,

    CONSTRAINT [PK_{migrationTableName}_Id] PRIMARY KEY ([Id])
);

";

                    command.ExecuteNonQuery();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $@"

ALTER TABLE [{migrationTableName}] 
    ADD CONSTRAINT [UNIQUE_{migrationTableName}_MigrationId] UNIQUE ([MigrationId]);

";

                    command.ExecuteNonQuery();
                }

                _writer.Write($"Created migration table '{migrationTableName}'");
            }
            catch (Exception exception)
            {
                throw new MigrationException($"Could not create migration table '{migrationTableName}'", exception);
            }
        }

        void LogMigration(ExclusiveDbConnection connection, IExecutableSqlMigration migration)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    $@"
INSERT INTO [{_migrationTableName}] (
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
                command.Parameters.Add("time", SqlDbType.DateTime2).Value = DateTime.Now;
                command.Parameters.Add("userName", SqlDbType.NVarChar).Value = Environment.UserName;
                command.Parameters.Add("userDomainName", SqlDbType.NVarChar).Value = Environment.UserDomainName;
                command.Parameters.Add("machineName", SqlDbType.NVarChar).Value = Environment.MachineName;

                command.ExecuteNonQuery();
            }
        }
    }
}