using System.Collections.Generic;
using System.Configuration;

namespace Migr8
{
    public class DatabaseMigrator
    {
        DatabaseMigratorCore _migrator;

        public DatabaseMigrator(string connectionStringOrConnectionStringName)
        {
            var connectionString = ConnectionStringFromSettings(connectionStringOrConnectionStringName)
                                ?? connectionStringOrConnectionStringName;

            _migrator = new DatabaseMigratorCore(new ConsoleWriter(), connectionString);
        }

        IEnumerable<IExecutableSqlMigration> GetMigrations()
        {
            throw new System.NotImplementedException();
        }

        static string ConnectionStringFromSettings(string connectionStringOrConnectionStringName)
        {
            return ConfigurationManager.ConnectionStrings[connectionStringOrConnectionStringName]?
                .ConnectionString;
        }
    }
}
