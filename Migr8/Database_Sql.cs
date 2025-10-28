using Migr8.SqlServer;

namespace Migr8
{
    /// <summary>
    /// SQL Server-specific database migration entry point.
    /// </summary>
    public static class Database
    {
        /// <summary>
        /// Executes the given migrations on the specified SQL Server database.
        /// </summary>
        /// <param name="connectionString">Specifies a connection string or the name of a connection string in the current application configuration file to use.</param>
        /// <param name="migrations">Supplies the migrations to be executed.</param>
        /// <param name="options">Optionally specifies some custom options to use.</param>
        public static void Migrate(string connectionString, Migrations migrations, Options options = null)
        {
            Migr8.Core.Database.Migrate(new SqlServerDb(), connectionString, migrations, options);
        }
    }
}