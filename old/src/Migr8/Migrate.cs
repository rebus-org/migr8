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
        public static void Database(string connectionStringOrConnectionStringKey,
            Action<IMigration> beforeExecute = null,
            Action<IMigration> afterExecuteSuccess = null,
            Action<IMigration, Exception> afterExecuteError = null,
            Options options = null)
        {
            var migrationOptions = options ?? new Options();
            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringOrConnectionStringKey];

            var connectionString = connectionStringSettings != null
                ? connectionStringSettings.ConnectionString
                : connectionStringOrConnectionStringKey;

            var callingAssembly = Assembly.GetCallingAssembly();

            Execute(connectionString, callingAssembly, beforeExecute, afterExecuteSuccess, afterExecuteError, migrationOptions);
        }

        private static void Execute(string connectionString, Assembly callingAssembly, Action<IMigration> beforeExecute, Action<IMigration> afterExecuteSuccess,
            Action<IMigration, Exception> afterExecuteError, Options options)
        {
            using (var migrate = new DatabaseMigrator(connectionString, new AssemblyScanner(callingAssembly), options))
            {
                if (beforeExecute != null) migrate.BeforeExecute += beforeExecute;
                if (afterExecuteSuccess != null) migrate.AfterExecuteSuccess += afterExecuteSuccess;
                if (afterExecuteError != null) migrate.AfterExecuteError += afterExecuteError;

                migrate.MigrateDatabase();
            }
        }
    }

    /// <summary>
    /// Configure options for Migr8
    /// </summary>
    public class Options
    {
        private string _versionTableName;
        private int? _commandTimeout;

        internal string VersionTableName
        {
            get { return _versionTableName; }
        }

        internal int? CommandTimeout
        {
            get { return _commandTimeout; }
        }

        /// <summary>
        /// This will force Migr8 to use the assigned tablename instead of extended properties
        /// </summary>
        /// <param name="tablename">Ex. DBVersion</param>
        /// <returns></returns>
        public Options UseVersionTableName(string tablename)
        {
            tablename = tablename.Trim('[', ']');
            _versionTableName = tablename;
            return this;
        }

        /// <summary>
        /// This will override the default command timeout of 30 seconds.
        /// </summary>
        /// <param name="timeoutInSeconds">SQL Command timeout in seconds</param>
        /// <returns></returns>
        public Options UseCommandTimeout(int timeoutInSeconds)
        {
            _commandTimeout = timeoutInSeconds;
            return this;
        }
    }
}