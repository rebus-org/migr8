using System.Linq;

namespace Migr8.DB
{
    public class TablePersister : IVersionPersister
    {
        private readonly string _tableName;

        public TablePersister(string tableName)
        {
            _tableName = tableName;
        }

        public void EnsureSchema(DatabaseContext context)
        {
            var sql = string.Format(@"SELECT * FROM INFORMATION_SCHEMA.TABLES 
				WHERE TABLE_SCHEMA = 'dbo' 
				AND  TABLE_NAME = '{0}'", _tableName);

            var properties = context.ExecuteQuery(sql);

            if (properties.Count == 0)
            {
                var newSchemaSql = string.Format(@"
                    CREATE TABLE dbo.{0}
					(
						MigrationVersion int NOT NULL
					)  ON [PRIMARY]
					INSERT INTO dbo.{0} VALUES('0')",
                    _tableName);

                context.ExecuteNonQuery(newSchemaSql);
            }
        }

        public int GetDatabaseVersionNumber(DatabaseContext context)
        {
            var currentVersion = context.GetSingleValue<int>(_tableName, "MigrationVersion");
            return currentVersion;
        }

        public void UpdateVersion(DatabaseContext context, int newVersion)
        {
            context.ExecuteNonQuery(string.Format("UPDATE dbo.{0} SET MigrationVersion = '{1}'", _tableName, newVersion));
        }
    }
}