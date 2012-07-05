using System;

namespace Migr8
{
    public class DatabaseMigrationException : ApplicationException
    {
        public DatabaseMigrationException(string message, params object[] objs)
            : base(string.Format(message, objs))
        {

        }

        public DatabaseMigrationException(Exception innerException, string message, params object[] objs)
            : base(string.Format(message, objs), innerException)
        {

        }
    }
}