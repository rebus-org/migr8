using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

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

            _writer.Write($"Migr8 has {executableSqlMigrations.Count} migrations - will execute now");

            while (true)
            {
                var didExecuteMigration = TryDoWork(executableSqlMigrations);

                if (!didExecuteMigration) break;
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
            var executedMigrationIds = connection.Select<string>("Id", $"SELECT [Id] FROM [{_migrationTableName}]");

            var remainingMigrations = migrations
                .Where(m => !executedMigrationIds.Contains(m.Id))
                .ToList();

            remainingMigrations.Sort(ByMigrationId);

            return remainingMigrations.FirstOrDefault();
        }

        static int ByMigrationId(IExecutableSqlMigration m1, IExecutableSqlMigration m2)
        {
            var id1 = m1.Id;
            var id2 = m2.Id;

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
    [Id] NVARCHAR(200) NOT NULL,
    [Sql] NVARCHAR(MAX) NOT NULL,
    [Time] DATETIME2 NOT NULL,
    [UserName] NVARCHAR(MAX) NOT NULL,
    [UserDomainName] NVARCHAR(MAX) NOT NULL,
    [MachineName] NVARCHAR(MAX) NOT NULL,

    CONSTRAINT [PK_{migrationTableName}_Id] PRIMARY KEY ([Id])
);

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
    [Id],
    [Sql],
    [Time],
    [UserName],
    [UserDomainName],
    [MachineName]
) VALUES (
    @id,
    @sql,
    @time,
    @userName,
    @userDomainName,
    @machineName
)
";
                command.Parameters.Add("id", SqlDbType.NVarChar, 200).Value = migration.Id;
                command.Parameters.Add("sql", SqlDbType.NVarChar).Value = migration.Sql;
                command.Parameters.Add("time", SqlDbType.DateTime2).Value = DateTime.Now;
                command.Parameters.Add("userName", SqlDbType.NVarChar).Value = Environment.UserName;
                command.Parameters.Add("userDomainName", SqlDbType.NVarChar).Value = Environment.UserDomainName;
                command.Parameters.Add("machineName", SqlDbType.NVarChar).Value = Environment.MachineName;

                command.ExecuteNonQuery();
            }
        }
    }
}