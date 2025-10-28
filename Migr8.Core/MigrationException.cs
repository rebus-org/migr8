using System;
using System.Runtime.Serialization;

namespace Migr8
{
    /// <summary>
    /// Exception that is thrown in case something goes wrong during the migration process.
    /// </summary>
    [Serializable]
    public class MigrationException : Exception
    {
        /// <summary>
        /// Happy serialization
        /// </summary>
        public MigrationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Constructs the exception with the given message and inner exception
        /// </summary>
        public MigrationException(string message, Exception exception)
            : base(message, exception)
        {
        }

        /// <summary>
        /// Constructs the exception with the given message
        /// </summary>
        public MigrationException(string message)
            : base(message)
        {
        }
    }
}