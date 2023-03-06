using Migr8.Internals;

namespace Migr8
{
    /// <summary>
    /// This is the entry point: Use Migr8 by calling <see cref="Migrate"/>, supplying a connection string (or a connection
    /// string name), some migrations (by calling <see cref="Migrations.FromAssemblyOf{T}"/>,
    /// or <see cref="Migrations.FromAssembly"/>), and possibly some <see cref="Options"/> if you want to customize something.
    /// Migrations are classes decorated with the <see cref="MigrationAttribute"/>, which must implement <see cref="ISqlMigration"/>
    /// in order to provide some SQL to be executed.
    /// </summary>
    public static partial class Database
    {
        /// <summary>
        /// Executes the given migrations on the specified database.
        /// </summary>
        /// <param name="connectionString">Specifies a connection string or the name of a connection string in the current application configuration file to use.</param>
        /// <param name="migrations">Supplies the migrations to be executed.</param>
        /// <param name="options">Optionally specifies some custom options to use.</param>
        public static void Migrate(string connectionString, Migrations migrations, Options options = null)
        {
            var database = GetDatabase();
            var optionsToUse = options ?? new Options();

            var migrator = new DatabaseMigratorCore(connectionString, optionsToUse, database);
            var executableSqlMigrations = migrations.GetMigrations();

            migrator.Execute(executableSqlMigrations);
        }
    }
}