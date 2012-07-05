using System;

namespace Migr8
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MigrationAttribute : Attribute
    {
        readonly int databaseVersionNumber;
        readonly string description;

        public MigrationAttribute(int databaseVersionNumber, string description)
        {
            this.databaseVersionNumber = databaseVersionNumber;
            this.description = description;
        }

        public int DatabaseVersionNumber
        {
            get { return databaseVersionNumber; }
        }

        public string Description
        {
            get { return description; }
        }
    }
}