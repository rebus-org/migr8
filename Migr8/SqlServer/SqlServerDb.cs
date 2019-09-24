using System;
using Migr8.Internals;
// ReSharper disable ArgumentsStyleNamedExpression

namespace Migr8.SqlServer
{
    class SqlServerDb : IDb
    {
        public IExclusiveDbConnection GetExclusiveDbConnection(string connectionString, Options options, bool useTransaction = true)
        {
            var connectionStringMutator = new ConnectionStringMutator(connectionString);

            var useManagedIdentity = connectionStringMutator.HasElement("Authentication", "ManagedIdentity", comparison: StringComparison.OrdinalIgnoreCase)
                || connectionStringMutator.HasElement("Authentication", "Active Directory Interactive", comparison: StringComparison.OrdinalIgnoreCase);

            if (useManagedIdentity)
            {
                if (connectionStringMutator.HasElement("Integrated Security", "SSPI", comparison: StringComparison.OrdinalIgnoreCase)
                    || connectionStringMutator.HasElement("Integrated Security", "true", comparison: StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("The connection string cannot be used with Authentication=ManagedIdentity, because it also contains Integrated Security = true or SSPI");
                }
            }

            var connectionStringToUse = connectionStringMutator
                .Without(k => string.Equals(k.Key, "Authentication", StringComparison.OrdinalIgnoreCase))
                .ToString();

            return new SqlServerExclusiveDbConnection(connectionStringToUse, options, useTransaction, useManagedIdentity);
        }
    }
}