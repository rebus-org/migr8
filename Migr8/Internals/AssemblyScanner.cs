using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Migr8.Internals
{
    class AssemblyScanner
    {
        readonly Assembly _assembly;

        public AssemblyScanner(Assembly assembly)
        {
            _assembly = assembly;
        }

        public IEnumerable<IExecutableSqlMigration> GetMigrations()
        {
            return _assembly
                .GetTypes()
                .Select(t => new
                {
                    Type = t,
                    Attribute = t.GetCustomAttributes(typeof(MigrationAttribute), false)
                        .Cast<MigrationAttribute>()
                        .FirstOrDefault()
                })
                .Where(a => a.Attribute != null)
                .Select(a => new
                {
                    Type = a.Type,
                    Attribute = a.Attribute,
                    Instance = CreateSqlMigrationInstance(a.Type)
                })
                .Select(a => CreateExecutableSqlMigration(a.Attribute, a.Instance))
                .ToList();
        }

        static ISqlMigration CreateSqlMigrationInstance(Type type)
        {
            if (!type.GetInterfaces().Contains(typeof(ISqlMigration)))
            {
                throw new MigrationException($"The type {type} does not implement {typeof(ISqlMigration)}");
            }

            try
            {
                return (ISqlMigration)Activator.CreateInstance(type);
            }
            catch (Exception exception)
            {
                throw new MigrationException($"Could not create instance of {type}", exception);
            }
        }

        static IExecutableSqlMigration CreateExecutableSqlMigration(MigrationAttribute attribute, ISqlMigration instance)
        {
            var id = $"{attribute.SequenceNumber}-{attribute.OptionalBranchSpecification ?? "master"}";
            var sql = instance.Sql;
            var description = attribute.Description;

            return new ExecutableSqlMigration(id, sql, description);
        }

        class ExecutableSqlMigration : IExecutableSqlMigration
        {
            public ExecutableSqlMigration(string id, string sql, string description)
            {
                Id = id;
                Sql = sql;
                Description = description;
            }

            public string Id { get; }
            public string Sql { get; }
            public string Description { get; }

            public override string ToString()
            {
                const int maxDisplayLength = 80;

                var sql = Sql.Length > maxDisplayLength 
                    ? Sql.Substring(0, maxDisplayLength) + "..." 
                    : Sql;

                return $"{Id}: {sql}";
            }
        }
    }
}