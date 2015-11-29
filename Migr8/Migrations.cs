using System.Collections.Generic;
using System.Reflection;
using Migr8.Internals;

namespace Migr8
{
    /// <summary>
    /// Wraps a set of database migrations. Get an instance by calling the static <see cref="FromThisAssembly"/>,
    /// <see cref="FromAssemblyOf{T}"/>, or <see cref="FromAssembly"/>
    /// </summary>
    public class Migrations
    {
        public static Migrations FromThisAssembly()
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            return GetFromAssembly(callingAssembly);
        }

        public static Migrations FromAssemblyOf<T>()
        {
            var assembly = typeof (T).Assembly;
            return GetFromAssembly(assembly);
        }

        public static Migrations FromAssembly(Assembly assembly)
        {
            return GetFromAssembly(assembly);
        }

        static Migrations GetFromAssembly(Assembly assembly)
        {
            var scanner = new AssemblyScanner(assembly);
            var migrations = scanner.GetMigrations();
            return new Migrations(migrations);
        }

        readonly List<IExecutableSqlMigration> _migrations = new List<IExecutableSqlMigration>();

        Migrations(IEnumerable<IExecutableSqlMigration> migrations)
        {
            _migrations.AddRange(migrations);
        }

        internal IEnumerable<IExecutableSqlMigration> GetMigrations()
        {
            return _migrations;
        }
    }
}