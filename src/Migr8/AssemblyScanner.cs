using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;

namespace Migr8
{
    public class AssemblyScanner : IProvideMigrations
    {
        readonly Assembly[] assemblies;

        public AssemblyScanner(params Assembly[] assemblies)
        {
            this.assemblies = assemblies;
        }

        public IMigration[] GetAllMigrations()
        {
            return assemblies.SelectMany(a => a.GetTypes())
                .Where(IsMigration)
                .Select(GenerateMigration)
                .ToArray();
        }

        IMigration GenerateMigration(Type type)
        {
            var attribute = GetMigrationAttribute(type);

            if (type.GetInterfaces().Contains(typeof(ISqlMigration)))
            {
                var instance = GetSqlMigrationInstance(type);

                return new SqlMigration
                           {
                               Description = attribute.Description,
                               TargetDatabaseVersion = attribute.DatabaseVersionNumber,
                               SqlStatements = Regex.Split(instance.Sql, @"^go *\;? *", RegexOptions.Multiline | RegexOptions.IgnoreCase)
                                   .Where(sql => !string.IsNullOrWhiteSpace(sql))
                                   .ToList(),
                           };
            }

            throw new InvalidOperationException(string.Format("The migration {0} does not implement ISqlMigration, which is currently required for a migration."));
        }

        ISqlMigration GetSqlMigrationInstance(Type type)
        {
            return (ISqlMigration) Activator.CreateInstance(type);
        }

        bool IsMigration(Type type)
        {
            return GetMigrationAttribute(type) != null;
        }

        static MigrationAttribute GetMigrationAttribute(Type type)
        {
            return type.GetCustomAttributes(typeof (MigrationAttribute), false)
                .Cast<MigrationAttribute>()
                .SingleOrDefault();
        }
    }

    class SqlMigration : IMigration
    {
        public int TargetDatabaseVersion { get; set; }
        public IEnumerable<string> SqlStatements { get; set; }
        public string Description { get; set; }
    }
}