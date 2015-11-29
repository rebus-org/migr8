using System;

namespace Migr8
{
    /// <summary>
    /// Decorate a class with this attribute to have it picked up by the assembly scanner when scanning
    /// for migrations using <see cref="Migrations.FromThisAssembly"/>, <see cref="Migrations.FromAssemblyOf{T}"/>,
    /// or <see cref="Migrations.FromAssembly"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MigrationAttribute : Attribute
    {
        /// <summary>
        /// Constructs the migration attribute with the given parameters
        /// </summary>
        /// <param name="sequenceNumber">Specifies the sequence in which the migration is to be executed. This will function as a global ordering of all the migrations, deterministically ensuring that migrations with higher numbers are always executed later.</param>
        /// <param name="description">Optionally supplies the migration with a description which will be added to the migration log.</param>
        /// <param name="optionalBranchSpecification">Optionally supplies the migration with a branch specification, which allows for multiple migrations to be positioned at the same point in the global sequence.</param>
        public MigrationAttribute(int sequenceNumber, string description = null, string optionalBranchSpecification = null)
        {
            SequenceNumber = sequenceNumber;
            Description = description;
            OptionalBranchSpecification = optionalBranchSpecification;
        }

        internal int SequenceNumber { get; }

        internal string Description { get; }

        internal string OptionalBranchSpecification { get; }
    }
}