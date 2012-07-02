using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Migr8
{
    public class DatabaseContext : IDisposable
    {
        readonly bool usingExternallyProvidedDbConnection;
        readonly IDbConnection dbConnection;
        IDbTransaction dbTransaction;

        public DatabaseContext(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
            usingExternallyProvidedDbConnection = true;
        }

        public DatabaseContext(string connectionString)
            : this(new SqlConnection(connectionString))
        {
            dbConnection.Open();
            usingExternallyProvidedDbConnection = false;
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

        public DatabaseContext Using(string databaseName)
        {
            ExecuteNonQuery("use " + databaseName);
            return this;
        }

        public void Commit()
        {
            if (dbTransaction == null)
            {
                throw new InvalidOperationException("Cannot commit when there's not transaction");
            }
            Console.WriteLine("TX: Commit");
            dbTransaction.Commit();
            dbTransaction = null;
        }

        public void Dispose()
        {
            if (dbTransaction != null)
            {
                Console.WriteLine("TX: Rollback");
                dbTransaction.Rollback();
                dbTransaction.Dispose();
                dbTransaction = null;
            }

            if (!usingExternallyProvidedDbConnection)
            {
                dbConnection.Dispose();
                Console.WriteLine("Connection disposed");
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
                    while (reader.NextResult())
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
    }
}