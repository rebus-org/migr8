using System;
using System.Runtime.Serialization;

namespace Migr8
{
    [Serializable]
    public class MigrationException : Exception
    {
        public MigrationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public MigrationException(string message, Exception exception)
            : base(message, exception)
        {
        }

        public MigrationException(string message)
            : base(message)
        {
        }
    }
}