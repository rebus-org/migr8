namespace Migr8.Internal
{
    public interface IProvideMigrations
    {
        IMigration[] GetAllMigrations();
    }
}