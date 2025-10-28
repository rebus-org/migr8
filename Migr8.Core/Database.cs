using Migr8.Internals;

namespace Migr8.Core
{
    /// <summary>
    /// This is the core database migration class that contains the shared migration logic.
    /// Database-specific implementations (SQL Server, PostgreSQL, MySQL) should call <see cref="Migrate"/> method.
    /// Migrations are classes decorated with the <see cref="MigrationAttribute"/>, which must implement <see cref="ISqlMigration"/>
    /// in order to provide some SQL to be executed.
    /// </summary>
    public static class Database
    {
        /// <summary>
        /// Executes the given migrations on the specified database using the provided database implementation.
        /// </summary>
        /// <param name="database">The database-specific implementation to use.</param>
        /// <param name="connectionString">Specifies a connection string or the name of a connection string in the current application configuration file to use.</param>
        /// <param name="migrations">Supplies the migrations to be executed.</param>
        /// <param name="options">Optionally specifies some custom options to use.</param>
        public static void Migrate(IDb database, string connectionString, Migrations migrations, Options options = null)
        {
            var optionsToUse = options ?? new Options();

            var migrator = new DatabaseMigratorCore(connectionString, optionsToUse, database);
            var executableSqlMigrations = migrations.GetMigrations();

            migrator.Execute(executableSqlMigrations);
        }
    }
}