using Migr8.Internals;
using Migr8.Npgsql.Postgres;

namespace Migr8
{
    public static partial class Database
    {
        internal static IDb GetDatabase()
        {
            return new PostgreSqlDb();
        }
    }
}