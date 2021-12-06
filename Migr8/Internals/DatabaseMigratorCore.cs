using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable EmptyGeneralCatchClause

namespace Migr8.Internals
{
    class DatabaseMigratorCore
    {
        readonly Options _options;
        readonly IWriter _writer;
        readonly string _connectionString;
        readonly IDb _db;
        readonly string _migrationTableName;

        public DatabaseMigratorCore(IWriter writer, string connectionString, string migrationTableName = null, IDb db = null)
        {
            _writer = writer;
            _connectionString = connectionString;
            _db = db ?? Database.GetDatabase();
            _migrationTableName = migrationTableName ?? Options.DefaultMigrationTableName;
            _options = new Options();

            _writer.Verbose($"Database migrator core initialized with connection string '{_connectionString}'");
            _writer.Verbose($"Storing migration log in table '{_migrationTableName}'");
            _writer.Verbose($"DB implementation: {_db}");
        }

        public DatabaseMigratorCore(string connectionString, Options options, IDb db)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _db = db ?? throw new ArgumentNullException(nameof(db));

            _migrationTableName = options.MigrationTableName;
            _writer = options.GetWriter();

            _writer.Verbose($"Database migrator core initialized with connection string '{_connectionString}'");
            _writer.Verbose($"Storing migration log in table '{_migrationTableName}'");
            _writer.Verbose($"DB implementation: {_db}");
        }

        public void Execute(IEnumerable<IExecutableSqlMigration> migrations)
        {
            var executableSqlMigrations = migrations.ToList();

            if (executableSqlMigrations.Count == 0)
            {
                _writer.Info("Found no migrations");
                return;
            }

            _writer.Info($"Migr8 found {executableSqlMigrations.Count} migrations");

            AssertHasNoDuplicateIds(executableSqlMigrations);

            var stopwatchTotal = Stopwatch.StartNew();

            while (true)
            {
                var didExecuteMigration = ExecuteNextMigration(executableSqlMigrations);

                if (!didExecuteMigration)
                {
                    _writer.Info(
                        $"No more migrations to run (execution took {stopwatchTotal.Elapsed.TotalSeconds:0.0} s)");
                    break;
                }
            }
        }

        void AssertHasNoDuplicateIds(IEnumerable<IExecutableSqlMigration> executableSqlMigrations)
        {
            var sqlMigrationsList = executableSqlMigrations.ToList();

            var migrationIds = sqlMigrationsList.Select(m => m.Id).Distinct();

            _writer.Verbose($"Found the following migration IDs: {string.Join(", ", migrationIds)}");

            _writer.Verbose("Checking for duplicate IDs among migrations");

            var duplicatedMigrations = sqlMigrationsList
                .GroupBy(m => m.Id)
                .ToList()
                .Where(g => g.Count() > 1)
                .ToList();

            if (duplicatedMigrations.Any())
            {
                var duplicatedIds = string.Join(", ", duplicatedMigrations.SelectMany(g => g.Select(m => m.Id)));

                throw new MigrationException(
                    $"Cannot execute migrations because the following migrations are duplicates: {duplicatedIds}");
            }
        }

        bool ExecuteNextMigration(List<IExecutableSqlMigration> migrations)
        {
            _writer.Verbose("Opening access to database");

            using var connection = _db.GetExclusiveDbConnection(_connectionString, _options, _writer);
            
            EnsureMigrationTableExists(connection);

            _writer.Verbose("Getting next migration to run...");

            var nextMigration = GetNextMigration(connection, migrations, _writer);

            if (nextMigration == null)
            {
                _writer.Verbose("Found no migration");
                return false;
            }

            _writer.Verbose($"Found migration {nextMigration.Id} - executing!");

            var executionStopwatch = Stopwatch.StartNew();
            try
            {

                if (nextMigration.Hints.Contains(Hints.NoTransaction))
                {
                    ExecuteMigrationWithSeparateConnection(nextMigration, connection);
                }
                else
                {
                    ExecuteMigration(nextMigration, connection, connection);
                }

                connection.Complete();
                    
                return true;
            }
            catch (Exception exception)
            {
                throw new MigrationException(
                    $"Could not execute migration with ID '{nextMigration.Id}': {nextMigration.Sql}", exception);
            }
            finally
            {
                _writer.Verbose(
                    $"Execution of migration {nextMigration.Id} took {executionStopwatch.Elapsed.TotalSeconds:0.0} s");
            }
        }

        void ExecuteMigrationWithSeparateConnection(IExecutableSqlMigration nextMigration, IExclusiveDbConnection connection)
        {
            using var separateConnection = _db.GetExclusiveDbConnection(_connectionString, _options, _writer, useTransaction: false);
            ExecuteMigration(nextMigration, connection, separateConnection);
            separateConnection.Complete();
        }

        void ExecuteMigration(IExecutableSqlMigration migration, IExclusiveDbConnection logConnection, IExclusiveDbConnection migrationConnection)
        {
            var id = migration.Id;
            var sql = migration.Sql;

            _writer.Verbose($"Inserting log row for migration {id}");

            LogMigration(logConnection, migration);

            const RegexOptions options = RegexOptions.Multiline
                                         | RegexOptions.IgnorePatternWhitespace
                                         | RegexOptions.IgnoreCase;

            const string searchPattern = @"^\s*GO\s* ($ | \-\- .*$)";

            var sqlStatements = Regex.Split(sql, searchPattern, options)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim(' ', '\r', '\n'))
                .ToList();

            _writer.Verbose($"Migration {id} contains {sqlStatements.Count} individual SQL statements");

            foreach (var sqlStatement in sqlStatements)
            {
                _writer.Verbose($"Executing statement: {sqlStatement}");

                try
                {
                    var hints = HintParser.ParseHints(migration.Hints);
                    var commandTimeout = hints.GetHint("sql-command-timeout")?.GetValueAsTimeSpan();

                    migrationConnection.ExecuteStatement(sqlStatement, commandTimeout);
                }
                catch (Exception exception)
                {
                    throw new MigrationException($"Error executing SQL statement: '{sqlStatement}'", exception);
                }
            }

            _writer.Info($"Migration {id} executed");
        }

        IExecutableSqlMigration GetNextMigration(IExclusiveDbConnection connection,
            List<IExecutableSqlMigration> migrations, IWriter writer)
        {
            var executedMigrationIds = connection.GetExecutedMigrationIds(_migrationTableName).ToList();

            writer.Verbose($"Database reports the following migrations have been executed: {string.Join(", ", executedMigrationIds)}");

            var remainingMigrations = migrations
                .Where(m => !executedMigrationIds.Contains(m.Id))
                .ToList();

            VerifyMigrationBandit(executedMigrationIds, remainingMigrations);

            var remainingMigrationsList = remainingMigrations
                .OrderBy(m => m.SequenceNumber).ThenBy(m => m.BranchSpecification)
                .ToList();

            writer.Verbose($"The following migrations remain in order: {string.Join(", ", remainingMigrationsList.Select(m => m.Id))}");

            var nextMigration = remainingMigrationsList.FirstOrDefault();

            return nextMigration;
        }

        static void VerifyMigrationBandit(IEnumerable<string> executedMigrationIds, List<IExecutableSqlMigration> remainingMigrations)
        {
            var executedMigrationsByBranch = executedMigrationIds
                .Select(MigrationId.FromString)
                .GroupBy(id => id.BranchSpecification)
                .ToList();

            var migrationsThatShouldHaveBeenExecutedByNow = remainingMigrations
                .Where(m => ShouldHaveBeenExecutedByNow(executedMigrationsByBranch, m))
                .ToList();

            if (migrationsThatShouldHaveBeenExecutedByNow.Any())
            {
                var ids = string.Join(", ", migrationsThatShouldHaveBeenExecutedByNow.Select(m => m.Id));

                throw new MigrationException(
                    $"Cannot execute migrations because migration the following migrations have NOT been executed by now, even though they should have been: {ids}");
            }
        }

        static bool ShouldHaveBeenExecutedByNow(IEnumerable<IGrouping<string, MigrationId>> executedMigrationsByBranch, IExecutableSqlMigration executableSqlMigration)
        {
            var migrationId = MigrationId.FromString(executableSqlMigration.Id);

            var migrationsForThisParticularBranch = executedMigrationsByBranch
                                                        .FirstOrDefault(b => b.Key == migrationId.BranchSpecification)
                                                        ?.ToList()
                                                    ?? new List<MigrationId>();

            return migrationsForThisParticularBranch.Any(id => id.CompareTo(migrationId) > 0);
        }

        class MigrationId : IComparable<MigrationId>
        {
            public static MigrationId FromString(string migrationId)
            {
                var tokens = migrationId.Split('-');

                return new MigrationId(int.Parse(tokens.First()), string.Join("-", tokens.Skip(1)));
            }

            MigrationId(int sequenceNumber, string branchSpecification)
            {
                SequenceNumber = sequenceNumber;
                BranchSpecification = branchSpecification;
            }

            public int SequenceNumber { get; }
            public string BranchSpecification { get; }

            public int CompareTo(MigrationId other)
            {
                var majorCompare = SequenceNumber.CompareTo(other.SequenceNumber);

                return majorCompare != 0
                    ? majorCompare
                    : string.Compare(BranchSpecification, other.BranchSpecification,
                        StringComparison.OrdinalIgnoreCase);
            }
        }

        static int CompareMigrationId(string id1, string id2)
        {
            var v1 = MigrationId.FromString(id1);
            var v2 = MigrationId.FromString(id2);

            return v1.CompareTo(v2);
        }

        void EnsureMigrationTableExists(IExclusiveDbConnection connection)
        {
            var tableNames = connection.GetTableNames();

            if (!tableNames.Contains(_migrationTableName))
            {
                _writer.Verbose($"Database does not contain migration log table '{_migrationTableName}' - will try to create it now");

                _writer.Verbose($"Database contains these tables: {string.Join(", ", tableNames)}");

                try
                {
                    CreateMigrationTable(_migrationTableName, connection);
                }
                catch (Exception)
                {
                    // if table creation failed, we might have run into a race with another process...
                    // ....therefore, just check if the table NOW exists:
                    try
                    {
                        _writer.Verbose("Could not create the table - checking if someone else created it");

                        if (connection.GetTableNames().Contains(_migrationTableName))
                        {
                            _writer.Verbose($"Database contains these tables: {string.Join(", ", tableNames)}");

                            _writer.Verbose($"The migration log table '{_migrationTableName}' was now found - we're good");
                            return;
                        }
                    }
                    catch { }

                    // if it didn't exist, throw the original exception
                    _writer.Verbose($"Could not find the migration log table '{_migrationTableName}' - re-throwing the exception caught when initally trying to create it");
                    throw;
                }
            }
        }

        void CreateMigrationTable(string migrationTableName, IExclusiveDbConnection connection)
        {
            try
            {
                connection.CreateMigrationTable(migrationTableName);

                _writer.Info($"Created migration table '{migrationTableName}'");
            }
            catch (Exception exception)
            {
                throw new MigrationException($"Could not create migration table '{migrationTableName}'", exception);
            }
        }

        void LogMigration(IExclusiveDbConnection connection, IExecutableSqlMigration migration)
        {
            connection.LogMigration(migration, _migrationTableName);
        }
    }
}