using System;
using Migr8.Internals;

namespace Migr8
{
    public class Options
    {
        public const string DefaultMigrationTableName = "__Migr8";

        /// <summary>
        /// Creates an options object fo the migrator to use.
        /// </summary>
        /// <param name="migrationTableName">Optionally specifies the name of the table in which executed migrations will be logged. Defaults to <see cref="DefaultMigrationTableName"/>.</param>
        /// <param name="logAction">Optionally specifies a log action to use, which can be used to track progress as the migrator is running. Defaults to printing output to the console.</param>
        public Options(
            string migrationTableName = DefaultMigrationTableName,
            Action<string> logAction = null)
        {
            MigrationTableName = migrationTableName;
            LogAction = logAction;
        }

        /// <summary>
        /// Sets the name used to track and log executed migrations. Defaults to <see cref="DefaultMigrationTableName"/>.
        /// </summary>
        internal string MigrationTableName { get; set; }

        /// <summary>
        /// Sets a log action to call when printing output. Defaults to printing to the console.
        /// </summary>
        internal Action<string> LogAction { get; set; }

        internal IWriter GetWriter()
        {
            if (LogAction != null)
            {
                return new LogActionWriter(LogAction);
            }

            return new ConsoleWriter();
        }
    }
}