namespace Migr8
{
    /// <summary>
    /// Represents an executable migration
    /// </summary>
    public class Migration
    {
        public int SequenceNumber { get; }
        public string BranchSpecification { get;  }

        public Migration(int sequenceNumber, string branchSpecification)
        {
            SequenceNumber = sequenceNumber;
            BranchSpecification = branchSpecification;
        }
    }
}