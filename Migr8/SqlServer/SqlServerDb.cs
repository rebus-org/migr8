using System;
using System.Linq;
using Migr8.Internals;
// ReSharper disable ArgumentsStyleNamedExpression

namespace Migr8.SqlServer
{
    class SqlServerDb : IDb
    {
        public IExclusiveDbConnection GetExclusiveDbConnection(string connectionString, Options options, IWriter writer, bool useTransaction = true)
        {
            const StringComparison ignoreCase = StringComparison.OrdinalIgnoreCase;
            
            var connectionStringMutator = new ConnectionStringMutator(connectionString);
            
            var authentication = connectionStringMutator.GetElement("Authentication", comparison: ignoreCase);
            var useManagedIdentity = string.Equals(authentication, "Active Directory Interactive", ignoreCase)
                                     || string.Equals(authentication, "Active Directory Integrated", ignoreCase);
            var tokenUrl = "";

            if (useManagedIdentity)
            {
                writer.Verbose($"Connection string with Authentication = {authentication} detected - configuring for use with managed identity");

                if (connectionStringMutator.HasElement("Integrated Security", "SSPI", comparison: ignoreCase)
                    || connectionStringMutator.HasElement("Integrated Security", "true", comparison: ignoreCase))
                {
                    throw new ArgumentException("The connection string cannot be used with Authentication=ManagedIdentity, because it also contains Integrated Security = true or SSPI");
                }

                var fullDatabaseHostName = connectionStringMutator.GetElement("server") ?? connectionStringMutator.GetElement("data source");
                var trimUntil = fullDatabaseHostName.TrimUntil(':');
                var trimAfter = trimUntil.TrimAfter(',');
                var databaseHostname = string.Join(".", trimAfter.Split('.').Skip(1));

                tokenUrl = $"https://{databaseHostname}";

                writer.Verbose($"Will retrieve access token from URL {tokenUrl}");
            }

            var connectionStringToUse = connectionStringMutator
                .Without(k => string.Equals(k.Key, "Authentication", ignoreCase))
                .ToString();

            return new SqlServerExclusiveDbConnection(connectionStringToUse, options, useTransaction, useManagedIdentity, tokenUrl);
        }
    }
}