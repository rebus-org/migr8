using System.Collections.Generic;

namespace Migr8
{
    public interface IMigration
    {
        /// <summary>
        /// Indicated the version number that this migration, once applied, will cause the database to have.
        /// </summary>
        int TargetDatabaseVersion { get; }

        /// <summary>
        /// Contains a series of SQL statements that will be executed, one at a time, within the same transaction.
        /// </summary>
        IEnumerable<string> SqlStatements { get; }

        /// <summary>
        /// Should contain a text that describes the purpose of this particular migration.
        /// </summary>
        string Description { get; }
    }
}