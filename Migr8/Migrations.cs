using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <summary>
        /// Gets all migrations found in the assembly calling this method.
        /// </summary>
        public static Migrations FromThisAssembly()
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            return GetFromAssembly(callingAssembly);
        }

        /// <summary>
        /// Gets all migrations found when scanning the assembly of the type <typeparamref name="T"/>.
        /// The type <typeparamref name="T"/> does not need to be a migration type, though.
        /// </summary>
        public static Migrations FromAssemblyOf<T>()
        {
            var assembly = typeof(T).Assembly;
            return GetFromAssembly(assembly);
        }

        /// <summary>
        /// Gets all migrations found in the specified assembly.
        /// </summary>
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

        /// <summary>
        /// Filters the migrations to include those that satisfy the given predicate
        /// </summary>
        public Migrations Where(Predicate<ExecutableMigration> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var netListOfMigrations = _migrations
                .Where(e =>
                {
                    var migration = new ExecutableMigration(e.SequenceNumber, e.BranchSpecification, e.Description, e.SqlMigration);

                    return predicate(migration);
                })
                .ToList();

            return new Migrations(netListOfMigrations);
        }

        readonly List<IExecutableSqlMigration> _migrations = new List<IExecutableSqlMigration>();

        internal Migrations(IEnumerable<IExecutableSqlMigration> migrations)
        {
            _migrations.AddRange(migrations);
        }

        internal IEnumerable<IExecutableSqlMigration> GetMigrations()
        {
            return _migrations;
        }

        internal static Migrations None => new Migrations(Enumerable.Empty<IExecutableSqlMigration>());
    }
}