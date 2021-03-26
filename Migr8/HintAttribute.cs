using System;

namespace Migr8
{
    /// <summary>
    /// Add this to a migration class to pass a hint to the execution engine
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class HintAttribute : Attribute
    {
        internal string Hint { get; }

        /// <summary>
        /// Creates the attribute with the given hint
        /// </summary>
        public HintAttribute(string hint)
        {
            Hint = hint ?? throw new ArgumentNullException(nameof(hint));
        }

        /// <summary>
        /// Creates the attribute with the given hint and value
        /// </summary>
        public HintAttribute(string hint, string value)
        {
            if (hint == null) throw new ArgumentException(nameof(hint));
            if (value == null) throw new ArgumentNullException(nameof(value));

            Hint = $"{hint}: {value}";
        }
    }
}