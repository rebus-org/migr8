using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Testy;

namespace Migr8.Test
{
    public abstract class DbFixtureBase : FixtureBase
    {
        const int DoesNotExist = 3701;
        
        readonly ConcurrentStack<IDisposable> _disposables = new();

        protected override void SetUp()
        {
            base.SetUp();

            ResetDatabase();
        }

        public static void ResetDatabase()
        {
            DropTable(Options.DefaultMigrationTableName);

            foreach (var tableName in GetTableNames())
            {
                DropTable(tableName);
            }
        }

        static void DropTable(string tableName)
        {
            using var connection = new SqlConnection(TestConfig.ConnectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = $"DROP TABLE [{tableName}]";

            Console.Write($"Dropping table [{tableName}] - ");
            try
            {
                command.ExecuteNonQuery();
                Console.WriteLine("OK");
            }
            catch (SqlException sqlException) when (sqlException.Number == DoesNotExist)
            {
                Console.WriteLine("Did not exist");
            }
        }

        [TearDown]
        public void TearDown()
        {
            while (_disposables.TryPop(out var disposable))
            {
                Console.WriteLine($"Disposing {disposable}");
                disposable.Dispose();
            }
        }

        protected virtual TDisposable Using<TDisposable>(TDisposable disposable) where TDisposable : IDisposable
        {
            _disposables.Push(disposable);
            return disposable;
        }

        protected static IEnumerable<string> GetTableNames(bool automaticallyExcludeMigrationTable = true)
        {
            var list = new List<string>();
            using (var connection = new SqlConnection(TestConfig.ConnectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT [TABLE_NAME] FROM [information_schema].[tables] WHERE [TABLE_TYPE] = 'BASE TABLE'";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add((string)reader["TABLE_NAME"]);
                        }
                    }
                }
            }

            if (automaticallyExcludeMigrationTable)
            {
                list.RemoveAll(name => name.StartsWith("__"));
            }

            list.Sort();

            return list;
        }
    }
}
