using System;
using Migr8.Internals;

namespace Migr8
{
    /// <summary>
    /// Specifies the options to use when running migrations. Use the constructor's optional arguments to customize things.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Default table name which will be used to log migrations.
        /// </summary>
        public const string DefaultMigrationTableName = "__Migr8";

        /// <summary>
        /// Default SQL command timeout in minutes
        /// </summary>
        public const int SqlCommandTimeoutMinutes = 10;

        /// <summary>
        /// Creates an options object fo the migrator to use.
        /// </summary>
        /// <param name="migrationTableName">Optionally specifies the name of the table in which executed migrations will be logged. Defaults to <see cref="DefaultMigrationTableName"/>.</param>
        /// <param name="logAction">Optionally specifies a log action to use, which can be used to track progress as the migrator is running. Defaults to printing output to the console.</param>
        /// <param name="verboseLogAction">Optionally specifies a log action to use for detailed logging, which can be used to track progress as the migrator is running. Defaults to not doing anything.</param>
        /// <param name="sqlCommandTimeout">Optionally specifies the command timeout to use for each command. Defaults to <see cref="SqlCommandTimeoutMinutes"/> minutes.</param>
        public Options(
            string migrationTableName = DefaultMigrationTableName,
            Action<string> logAction = null,
            Action<string> verboseLogAction = null,
            TimeSpan? sqlCommandTimeout = null
        )
        {
            MigrationTableName = migrationTableName;
            LogAction = logAction;
            VerboseLogAction = verboseLogAction;
            SqlCommandTimeout = sqlCommandTimeout ?? TimeSpan.FromMinutes(SqlCommandTimeoutMinutes);
        }

        /// <summary>
        /// Gets the name of the table in which executed migrations will be logged.
        /// </summary>
        public string MigrationTableName { get; }

        /// <summary>
        /// Gets the log action to use.
        /// </summary>
        public Action<string> LogAction { get; }

        /// <summary>
        /// Gets the verbose log action to use.
        /// </summary>
        public Action<string> VerboseLogAction { get; }

        /// <summary>
        /// Gets the SQL command timeout to use for each command.
        /// </summary>
        public TimeSpan SqlCommandTimeout { get; }

        /// <summary>
        /// Gets a writer for logging.
        /// </summary>
        public IWriter GetWriter() => new LogActionWriter(LogAction ?? LogToConsole, VerboseLogAction ?? DoNothing);

        static void LogToConsole(string text) => Console.WriteLine(text);

        static void DoNothing(string text)
        {
        }
    }
}