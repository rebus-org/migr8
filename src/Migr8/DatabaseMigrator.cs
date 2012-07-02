using System;
using System.Data;
using System.Data.SqlClient;

namespace Migr8
{
    public class DatabaseMigrator : IDisposable
    {
        readonly bool ownsTheDbConnection;
        readonly IDbConnection dbConnection;

        public DatabaseMigrator(IDbConnection dbConnection)
            : this(dbConnection, false)
        {
        }

        public DatabaseMigrator(string connectionString)
            : this(new SqlConnection(connectionString), true)
        {
        }

        DatabaseMigrator(IDbConnection dbConnection, bool ownsTheDbConnection)
        {
            this.ownsTheDbConnection = ownsTheDbConnection;
            this.dbConnection = dbConnection;

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
                dbConnection.Close();
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
                context.NewTransaction();

                var sql = string.Format("select * from sys.extended_properties where [class] = 0 and [name] = '{0}'",
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
