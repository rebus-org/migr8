using System.Reflection;

namespace Migr8
{
    public class AssemblyScanner : IProvideMigrations
    {
        readonly Assembly[] assemblies;

        public AssemblyScanner(params Assembly[] assemblies)
        {
            this.assemblies = assemblies;
        }

        public IMigration[] GetAllMigrations()
        {
            return new IMigration[0];
        }
    }
}