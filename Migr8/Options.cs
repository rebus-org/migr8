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
        /// Specifies the default table name which will be used to log migrations.
        /// </summary>
        public const string DefaultMigrationTableName = "__Migr8";

        /// <summary>
        /// Creates an options object fo the migrator to use.
        /// </summary>
        /// <param name="migrationTableName">Optionally specifies the name of the table in which executed migrations will be logged. Defaults to <see cref="DefaultMigrationTableName"/>.</param>
        /// <param name="logAction">Optionally specifies a log action to use, which can be used to track progress as the migrator is running. Defaults to printing output to the console.</param>
        /// <param name="verboseLogAction">Optionally specifies a log action to use for detailed logging, which can be used to track progress as the migrator is running. Defaults to not doing anything.</param>
        public Options(
            string migrationTableName = DefaultMigrationTableName,
            Action<string> logAction = null,
            Action<string> verboseLogAction = null)
        {
            MigrationTableName = migrationTableName;
            LogAction = logAction;
            VerboseLogAction = verboseLogAction;
        }

        internal string MigrationTableName { get; }

        internal Action<string> LogAction { get; }

        internal Action<string> VerboseLogAction { get; }

        internal IWriter GetWriter()
        {
            return new LogActionWriter(LogAction ?? LogToConsole, VerboseLogAction ?? DoNothing);
        }

        static void LogToConsole(string text)
        {
            Console.WriteLine(text);
        }

        static void DoNothing(string text)
        {
        }
    }
}