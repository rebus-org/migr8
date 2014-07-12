using System;
using System.Configuration;
using System.Reflection;
using Migr8.Internal;

namespace Migr8
{
    public class Migrate
    {
        /// <summary>
        /// Picks up migrations from the calling assembly and executes them against the database specified by the given connection string.
        /// The connection string can be specified either by providing a key of a connection string in the connectionStrings configuration
        /// section of your app.config/web.config, or by providing the raw connection string.
        /// </summary>
        public static void Database(string connectionStringOrConnectionStringKey, Action<IMigration> beforeExecute = null, Action<IMigration> afterExecuteSuccess = null, Action<IMigration, Exception> afterExecuteError = null)
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringOrConnectionStringKey];

            var connectionString = connectionStringSettings != null
                                       ? connectionStringSettings.ConnectionString
                                       : connectionStringOrConnectionStringKey;

            var callingAssembly = Assembly.GetCallingAssembly();

            Execute(connectionString, callingAssembly, beforeExecute, afterExecuteSuccess, afterExecuteError);
        }

        static void Execute(string connectionString, Assembly callingAssembly, Action<IMigration> beforeExecute, Action<IMigration> afterExecuteSuccess, Action<IMigration, Exception> afterExecuteError)
        {
            using (var migrate = new DatabaseMigrator(connectionString, new AssemblyScanner(callingAssembly)))
            {
                if (beforeExecute != null) migrate.BeforeExecute += beforeExecute;
                if (afterExecuteSuccess != null) migrate.AfterExecuteSuccess += afterExecuteSuccess;
                if (afterExecuteError != null) migrate.AfterExecuteError += afterExecuteError;

                migrate.MigrateDatabase();
            }
        }
    }
}