using System.Configuration;
using Migr8.Internals;

namespace Migr8
{
    public class Database
    {
        public static void Migrate(string connectionStringOrConnectionStringName, Migrations migrations, Options options = null)
        {
            options = options ?? new Options();

            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringOrConnectionStringName]?.ConnectionString
                                ?? connectionStringOrConnectionStringName;

            var migrator = new DatabaseMigratorCore(
                migrationTableName: options.MigrationTableName,
                writer: options.GetWriter(),
                connectionString: connectionString);

            var executableSqlMigrations = migrations.GetMigrations();

            migrator.Execute(executableSqlMigrations);
        }
    }
}
