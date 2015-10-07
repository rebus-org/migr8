using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Migr8.DB;
using Migr8.Internal;

namespace Migr8
{
    public class DatabaseMigrator : IDisposable
    {
        private readonly bool _ownsTheDbConnection;
        private readonly IProvideMigrations _provideMigrations;
        private readonly IDbConnection _dbConnection;
        private readonly IVersionPersister _communicator;
        private readonly int? _transactionTimeout;

        public event Action<IMigration> BeforeExecute = delegate { };
        public event Action<IMigration> AfterExecuteSuccess = delegate { };
        public event Action<IMigration, Exception> AfterExecuteError = delegate { };

        public DatabaseMigrator(IDbConnection dbConnection, IProvideMigrations provideMigrations, Options options)
            : this(dbConnection, false, provideMigrations, options)
        {
        }

        public DatabaseMigrator(string connectionString, IProvideMigrations provideMigrations, Options options)
            : this(CreateDbConnection(connectionString), true, provideMigrations, options)
        {
        }

        private static SqlConnection CreateDbConnection(string connectionString)
        {
            try
            {
                return new SqlConnection(connectionString);
            }
            catch (Exception e)
            {
                var errorMessage = string.Format("Could not create SQL connection using the specified connection string: '{0}'", connectionString);

                throw new ArgumentException(errorMessage, e);
            }
        }

        private DatabaseMigrator(IDbConnection dbConnection, bool ownsTheDbConnection, IProvideMigrations provideMigrations, Options options)
        {
            _ownsTheDbConnection = ownsTheDbConnection;
            _provideMigrations = provideMigrations;
            _dbConnection = dbConnection;

            if (options.VersionTableName != null)
            {
                _communicator = new TablePersister(options.VersionTableName);
            }

            if (options.CommandTimeout != null)
            {
                _transactionTimeout = options.CommandTimeout;
            }

            if (_communicator == null) //use default
            {
                _communicator = new ExtendedPropertiesPersister();
            }

            if (ownsTheDbConnection)
            {
                dbConnection.Open();
            }
        }

        public void Dispose()
        {
            if (_ownsTheDbConnection)
            {
                _dbConnection.Close();
                _dbConnection.Dispose();
            }
        }

        public void MigrateDatabase()
        {
            try
            {
                EnsureDatabaseHasVersionMetaData();

                var databaseVersionNumber = GetDatabaseVersionNumber();

                var allMigrations = _provideMigrations
                    .GetAllMigrations();
                var migrationsToExecute = allMigrations
                    .Where(m => m.TargetDatabaseVersion > databaseVersionNumber)
                    .ToList();

                ValidateSequence(databaseVersionNumber, migrationsToExecute);

                foreach (var migration in migrationsToExecute.OrderBy(m => m.TargetDatabaseVersion))
                {
                    ExecuteMigration(migration);
                }
            }
            catch (Exception e)
            {
                throw new DatabaseMigrationException(e, "Something bad happened during migration");
            }
        }

        void ValidateSequence(int initialDatabaseVersionNumber, IEnumerable<IMigration> migrationsToExecute)
        {
            var expectedVersionNumberOfMigration = initialDatabaseVersionNumber + 1;
            foreach (var migration in migrationsToExecute.OrderBy(m => m.TargetDatabaseVersion))
            {
                if (migration.TargetDatabaseVersion > expectedVersionNumberOfMigration)
                {
                    throw new DatabaseMigrationException(
                        "Sequence of migrations seems to be broken! A migration is missing that would bring the database to version {0}",
                        expectedVersionNumberOfMigration);
                }

                if (migration.TargetDatabaseVersion < expectedVersionNumberOfMigration)
                {
                    throw new DatabaseMigrationException(
                        "Sequence contains more than one migration that would bring the database to version {0}",
                        migration.TargetDatabaseVersion);
                }

                expectedVersionNumberOfMigration++;
            }
        }

        void ExecuteMigration(IMigration migration)
        {
            BeforeExecute(migration);

            try
            {
                using (var context = new DatabaseContext(_dbConnection, _transactionTimeout))
                {
                    context.NewTransaction();

                    foreach (var sqlStatement in migration.SqlStatements)
                    {
                        try
                        {
                            context.ExecuteNonQuery(sqlStatement);
                        }
                        catch (Exception e)
                        {
                            throw new DatabaseMigrationException(e,
                                @"The following SQL could not be executed:

{0}

Exception:

{1}",
                                              sqlStatement, e);
                        }
                    }

                    var currentVersion = GetDatabaseVersionNumber(context);
                    var newVersion = currentVersion + 1;

                    _communicator.UpdateVersion(context, newVersion);

                    context.Commit();
                }

                AfterExecuteSuccess(migration);
            }
            catch (Exception e)
            {
                AfterExecuteError(migration, e);

                throw new DatabaseMigrationException(e, "The migration {0} (db version -> {1}) could not be executed", migration.Description, migration.TargetDatabaseVersion);
            }
        }


        private int GetDatabaseVersionNumber()
        {
            using (var context = new DatabaseContext(_dbConnection))
            {
                return GetDatabaseVersionNumber(context);
            }
        }

        private int GetDatabaseVersionNumber(DatabaseContext context)
        {
            return _communicator.GetDatabaseVersionNumber(context);
        }

        private void EnsureDatabaseHasVersionMetaData()
        {
            using (var context = new DatabaseContext(_dbConnection))
            {
                context.NewTransaction();
                _communicator.EnsureSchema(context);
                context.Commit();
            }
        }
    }
}
