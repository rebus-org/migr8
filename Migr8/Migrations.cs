using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Migr8.Internals;
using Migr8.Internals.Scanners;

namespace Migr8
{
    /// <summary>
    /// Wraps a set of database migrations. Get an instance by calling the static
    /// <see cref="FromAssemblyOf{T}"/> or <see cref="FromAssembly"/>
    /// </summary>
    public class Migrations
    {
        /// <summary>
        /// Gets migrations from files in the specified directory and its subdirectories. Scans for files whose name
        /// matches the pattern "&lt;sequence-number&gt;-&lt;branch-specification&gt;.sql", e.g.
        /// "1-master.sql", "2-master-sql", "0001-feature-awesomeness.sql", etc. are valid migration names.
        /// </summary>
        public static Migrations FromFilesIn(string directory)
        {
            return GetFromDirectory(directory);
        }

        /// <summary>
        /// Gets migrations from files in the current directory and its subdirectories. Scans for files whose name
        /// matches the pattern "&lt;sequence-number&gt;-&lt;branch-specification&gt;.sql", e.g.
        /// "1-master.sql", "2-master-sql", "0001-feature-awesomeness.sql", etc. are valid migration names.
        /// </summary>
        public static Migrations FromFilesInCurrentDirectory()
        {
#if NET45
            return GetFromDirectory(AppDomain.CurrentDomain.BaseDirectory);
#else
            return GetFromDirectory(AppContext.BaseDirectory);
#endif
        }

        /// <summary>
        /// Gets all migrations found when scanning the assembly of the type <typeparamref name="T"/>.
        /// The type <typeparamref name="T"/> does not need to be a migration type, though.
        /// </summary>
        public static Migrations FromAssemblyOf<T>()
        {
            var assembly = typeof(T).GetTypeInfo().Assembly;
            return GetFromAssembly(assembly);
        }

        /// <summary>
        /// Gets all migrations found in the specified assembly.
        /// </summary>
        public static Migrations FromAssembly(Assembly assembly)
        {
            return GetFromAssembly(assembly);
        }

        static Migrations GetFromDirectory(string directory)
        {
            var scanner = new DirectoryScanner(directory);
            var migrations = scanner.GetMigrations();
            return new Migrations(migrations);
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

        /// <summary>
        /// Gets the contained migrations in a form that can be used to look at
        /// </summary>
        public List<ExecutableMigration> ToList()
        {
            return _migrations
                .Select(e => new ExecutableMigration(e.SequenceNumber, e.BranchSpecification, e.Description, e.SqlMigration))
                .ToList();
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