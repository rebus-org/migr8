using System;
using System.Data;
using System.Data.SqlClient;

namespace Migr8
{
    public class DatabaseMigrator : IDisposable
    {
        readonly bool ownsTheDbConnection;
        readonly IDbConnection dbConnection;
        readonly string databaseName;

        public DatabaseMigrator(IDbConnection dbConnection, string databaseName)
            : this(dbConnection, databaseName, false)
        {
        }

        public DatabaseMigrator(string connectionString, string databaseName)
            : this(new SqlConnection(connectionString), databaseName, true)
        {
        }

        DatabaseMigrator(IDbConnection dbConnection, string databaseName, bool ownsTheDbConnection)
        {
            this.ownsTheDbConnection = ownsTheDbConnection;
            this.dbConnection = dbConnection;
            this.databaseName = databaseName;

            if (ownsTheDbConnection)
            {
                dbConnection.Open();
            }
        }

        public void Dispose()
        {
            if (ownsTheDbConnection)
            {
                Console.WriteLine("Disposing connection");
                dbConnection.Dispose();
            }
            else
            {
                Console.WriteLine("Didn't dispose connection because it was provided from the outside");
            }
        }

        public void MigrateDatabase()
        {
            EnsureDatabaseHasVersionMetaData();
        }

        void EnsureDatabaseHasVersionMetaData()
        {
            using (var context = new DatabaseContext(dbConnection))
            {
                context
                    .Using(databaseName)
                    .NewTransaction();

                var sql = string.Format("select * from sys.extended_properties where [class] = '0' and [name] = '{0}'",
                                        ExtProp.DatabaseVersion);

                var properties = context.ExecuteQuery(sql);

                if (properties.Count == 0)
                {
                    context.ExecuteNonQuery(string.Format("exec sys.sp_addextendedproperty @name=N'{0}', @value=N'{1}'", ExtProp.DatabaseVersion, "1"));
                }

                context.Commit();
            }
        }
    }
}
