using System;
using Migr8.Internals;

namespace Migr8
{
    public class Options
    {
        public const string DefaultMigrationTableName = "__Migr8";

        public Options()
        {
            MigrationTableName = DefaultMigrationTableName;
        }

        /// <summary>
        /// Sets the name used to track and log executed migrations. Defaults to <see cref="DefaultMigrationTableName"/>.
        /// </summary>
        public string MigrationTableName { get; set; }

        /// <summary>
        /// Sets a log action to call when printing output. Defaults to printing to the console.
        /// </summary>
        public Action<string> LogAction { get; set; }

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