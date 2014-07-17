namespace Migr8.DB
{
    public interface IVersionPersister
    {
        void EnsureSchema(DatabaseContext context);
        int GetDatabaseVersionNumber(DatabaseContext context);
        void UpdateVersion(DatabaseContext context, int newVersion);
    }
}