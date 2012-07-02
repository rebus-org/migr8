namespace Migr8
{
    public interface IProvideMigrations
    {
        IMigration[] GetAllMigrations();
    }
}