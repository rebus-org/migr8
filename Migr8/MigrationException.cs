using System;
#if NET45
using System.Runtime.Serialization;

#endif

namespace Migr8
{
    /// <summary>
    /// Exception that is thrown in case something goes wrong during the migration process.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class MigrationException : Exception
    {
#if NET45
        /// <summary>
        /// Happy serialization
        /// </summary>
        public MigrationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif

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