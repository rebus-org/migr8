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
        public MigrationAttribute(int sequenceNumber, string description, string optionalBranchSpecification)
        {
            if (description == null) throw new ArgumentNullException(nameof(description));

            SequenceNumber = sequenceNumber;
            Description = description;
            OptionalBranchSpecification = optionalBranchSpecification;
        }

        internal int SequenceNumber { get; }

        internal string Description { get; }

        internal string OptionalBranchSpecification { get; }
    }
}