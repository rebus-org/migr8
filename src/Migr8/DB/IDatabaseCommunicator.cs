namespace Migr8.DB
{
    public interface IDatabaseCommunicator
    {
        void EnsureSchema(DatabaseContext context);
        int GetDatabaseVersionNumber(DatabaseContext context);
        void UpdateVersion(DatabaseContext context, int newVersion);
    }
}