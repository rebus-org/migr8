using System.Reflection;

namespace Migr8
{
    public class Migrate
    {
        /// <summary>
        /// Picks up migrations from the calling assembly and executes them against the database specified by the given connection string.
        /// </summary>
        public static void Database(string connectionString)
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            Execute(connectionString, callingAssembly);
        }

        static void Execute(string connectionString, Assembly callingAssembly)
        {
            using (var migrate = new DatabaseMigrator(connectionString, new AssemblyScanner(callingAssembly)))
            {
                migrate.MigrateDatabase();
            }
        }
    }
}