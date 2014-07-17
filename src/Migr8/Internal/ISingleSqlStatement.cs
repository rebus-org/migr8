namespace Migr8.Internal
{
    /// <summary>
    /// Implement this in a class and make sure the class is decorated with <see cref="MigrationAttribute"/>
    /// to have the <see cref="AssemblyScanner"/> pick it up.
    /// </summary>
    public interface ISqlMigration
    {
        /// <summary>
        /// Return SQL script to be executed. You can have the script executed as multiple
        /// SQL statements by using the 'go' command.
        /// </summary>
        string Sql { get; }
    }
}