using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Migr8.Internals
{
    class ExclusiveDbConnection : IDisposable
    {
        readonly SqlConnection _connection;
        readonly SqlTransaction _transaction;

        public ExclusiveDbConnection(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction(IsolationLevel.Serializable);
        }

        public void Dispose()
        {
            _transaction.Dispose();
            _connection.Dispose();
        }

        public void Complete()
        {
            _transaction.Commit();
        }

        public HashSet<string> GetTableNames()
        {
            var tableNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            using (var command = CreateCommand())
            {
                command.CommandText = "SELECT [TABLE_NAME] FROM [information_schema].[tables]";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tableNames.Add((string)reader["TABLE_NAME"]);
                    }
                }
            }

            return tableNames;
        }

        public SqlCommand CreateCommand()
        {
            var sqlCommand = _connection.CreateCommand();
            sqlCommand.Transaction = _transaction;
            return sqlCommand;
        }

        public IEnumerable<T> Select<T>(string columnName, string sqlQuery)
        {
            var results = new List<T>();

            using (var command = CreateCommand())
            {
                command.CommandText = sqlQuery;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add((T) reader[columnName]);
                    }
                }
            }

            return results;
        }
    }
}