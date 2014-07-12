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
        readonly bool ownsTheDbConnection;
        readonly IProvideMigrations provideMigrations;
        readonly IDbConnection dbConnection;
        private IDatabaseCommunicator communicator;

        public event Action<IMigration> BeforeExecute = delegate { };
        public event Action<IMigration> AfterExecuteSuccess = delegate { };
        public event Action<IMigration, Exception> AfterExecuteError = delegate { };

        public DatabaseMigrator(IDbConnection dbConnection, IProvideMigrations provideMigrations)
            : this(dbConnection, false, provideMigrations)
        {
        }

        public DatabaseMigrator(string connectionString, IProvideMigrations provideMigrations)
            : this(CreateDbConnection(connectionString), true, provideMigrations)
        {
        }

        static SqlConnection CreateDbConnection(string connectionString)
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

        private DatabaseMigrator(IDbConnection dbConnection, bool ownsTheDbConnection, IProvideMigrations provideMigrations)
        {
            this.ownsTheDbConnection = ownsTheDbConnection;
            this.provideMigrations = provideMigrations;
            this.dbConnection = dbConnection;
            communicator = new ExtendedPropertiesCommunicator();

            if (ownsTheDbConnection)
            {
                dbConnection.Open();
            }
        }

        public void Dispose()
        {
            if (ownsTheDbConnection)
            {
                dbConnection.Close();
                dbConnection.Dispose();
            }
        }

        public void MigrateDatabase()
        {
            try
            {
                EnsureDatabaseHasVersionMetaData();

                var databaseVersionNumber = GetDatabaseVersionNumber();

                var migrationsToExecute = provideMigrations
                    .GetAllMigrations()
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
                using (var context = new DatabaseContext(dbConnection))
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

                    communicator.UpdateVersion(context, newVersion);
                
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
            using (var context = new DatabaseContext(dbConnection))
            {
                return GetDatabaseVersionNumber(context);
            }
        }

        private int GetDatabaseVersionNumber(DatabaseContext context)
        {
            return communicator.GetDatabaseVersionNumber(context);
        }

        private void EnsureDatabaseHasVersionMetaData()
        {
            using (var context = new DatabaseContext(dbConnection))
            {
                context.NewTransaction();
                communicator.EnsureSchema(context);
                context.Commit();
            }
        }
    }
}
