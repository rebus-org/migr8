using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace Migr8
{
    public class DatabaseContext : IDisposable
    {
        static int contextCounter = 0;

        readonly bool dbConnectionIsOwned;
        readonly IDbConnection dbConnection;
        IDbTransaction dbTransaction;
        readonly int instanceId = Interlocked.Increment(ref contextCounter);

        public DatabaseContext(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
            dbConnectionIsOwned = false;
        }

        public DatabaseContext(string connectionString)
            : this(new SqlConnection(connectionString))
        {
            dbConnectionIsOwned = true;
            dbConnection.Open();
        }

        public DatabaseContext NewTransaction()
        {
            if (dbTransaction != null)
            {
                throw new InvalidOperationException("Transaction already started!");
            }
            dbTransaction = dbConnection.BeginTransaction();
            return this;
        }

        public void Commit()
        {
            if (dbTransaction == null)
            {
                throw new InvalidOperationException("Cannot commit when no transaction has been started!");
            }
            Log("TX: Commit");
            dbTransaction.Commit();
            dbTransaction = null;
        }

        public void Dispose()
        {
            try
            {
                if (dbTransaction != null)
                {
                    Log("TX: Rollback");
                    try
                    {
                        dbTransaction.Rollback();
                    }
                    catch { }

                    dbTransaction.Dispose();
                    dbTransaction = null;
                }
            }
            catch (Exception e)
            {
                Log("Error during TX Rollback: {0}", e);
                throw;
            }
            finally
            {
                if (dbConnectionIsOwned)
                {
                    dbConnection.Close();
                    dbConnection.Dispose();
                    Log("Connection disposed");
                }
            }
        }

        public List<Dictionary<string, object>> ExecuteQuery(string sql)
        {
            using (var command = dbConnection.CreateCommand())
            {
                if (dbTransaction != null)
                {
                    command.Transaction = dbTransaction;
                }

                command.CommandText = sql;
                var rows = new List<Dictionary<string, object>>();

                using (var reader = command.ExecuteReader())
                {
                    var fieldCount = reader.FieldCount;
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (var index = 0; index < fieldCount; index++)
                        {
                            row[reader.GetName(index)] = reader.GetValue(index);
                        }
                        rows.Add(row);
                    }
                }

                return rows;
            }
        }

        public void ExecuteNonQuery(string sql, bool ignoreException = false)
        {
            using (var command = dbConnection.CreateCommand())
            {
                if (dbTransaction != null)
                {
                    command.Transaction = dbTransaction;
                }

                command.CommandText = sql;

                try
                {
                    command.ExecuteNonQuery();
                }
                catch
                {
                    if (ignoreException) return;
                    throw;
                }
            }
        }

        void Log(string message, params object[] objs)
        {
            Console.WriteLine("CTX " + instanceId + ": " + message, objs);
        }

        public void KillConnections(string databaseName)
        {
            ExecuteNonQuery(
                string.Format(
                    @"
DECLARE @DatabaseName nvarchar(50)
SET @DatabaseName = N'{0}'

DECLARE @SQL varchar(max)

SELECT @SQL = COALESCE(@SQL,'') + 'Kill ' + Convert(varchar, SPId) + ';'
FROM MASTER..SysProcesses
WHERE DBId = DB_ID(@DatabaseName) AND SPId <> @@SPId

--SELECT @SQL 
EXEC(@SQL)
",
                    databaseName));
        }
    }
}