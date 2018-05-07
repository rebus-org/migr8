namespace Migr8
{
    /// <summary>
    /// Implement this interface in a class decorated with <see cref="MigrationAttribute"/> to create a migration.
    /// The returned SQL string can contain multiple statements by using the "GO" command, just like you do when
    /// you're using SQL Server Management Studio.
    /// </summary>
    public interface ISqlMigration
    {
        /// <summary>
        /// Should return one or more SQL statements. Multiple statements can be separated by GO.
        /// </summary>
        string Sql { get; }
    }
}