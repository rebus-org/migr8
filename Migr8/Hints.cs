namespace Migr8
{
    /// <summary>
    /// Constants for hints that can be handled by Migr8
    /// </summary>
    public static class Hints
    {
        /// <summary>
        /// When a migration is tagged with this hint, it is executed on its own connection without a transaction.
        /// This makes sense only when the migration is idempotent and thus safe to retry.
        /// </summary>
        public const string NoTransaction = "no-transaction";
    }
}