namespace Migr8
{
    /// <summary>
    /// Represents an executable migration
    /// </summary>
    public class ExecutableMigration
    {
        /// <summary>
        /// Gets the sequence number found on the <see cref="MigrationAttribute"/>
        /// </summary>
        public int SequenceNumber { get; }

        /// <summary>
        /// Gets the branch specification found on the <see cref="MigrationAttribute"/> (or "master" if the branch specification was not set)
        /// </summary>
        public string BranchSpecification { get;  }

        /// <summary>
        /// Gets the description found on the <see cref="MigrationAttribute"/>
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the SQL migration instance where the <see cref="MigrationAttribute"/> was found
        /// </summary>
        public ISqlMigration SqlMigration { get; }

        internal ExecutableMigration(int sequenceNumber, string branchSpecification, string description, ISqlMigration sqlMigration)
        {
            SequenceNumber = sequenceNumber;
            BranchSpecification = branchSpecification;
            Description = description;
            SqlMigration = sqlMigration;
        }
    }
}