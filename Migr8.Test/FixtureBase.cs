using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using NUnit.Framework;

namespace Migr8.Test
{
    public abstract class FixtureBase
    {
        const int DoesNotExist = 3701;
        readonly ConcurrentStack<IDisposable> _disposables = new ConcurrentStack<IDisposable>();

        [SetUp]
        public void InnerSetUp()
        {
            ResetDatabase();

            SetUp();
        }

        static void ResetDatabase()
        {
            DropTable(DatabaseMigratorCore.DefaultMigrationTableName);

            foreach (var tableName in GetTableNames())
            {
                DropTable(tableName);
            }
        }

        static void DropTable(string tableName)
        {
            using (var connection = new SqlConnection(TestConfig.ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
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
            }
        }

        protected virtual void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
            IDisposable disposable;

            while (_disposables.TryPop(out disposable))
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
                    command.CommandText = "SELECT [TABLE_NAME] FROM [information_schema].[tables]";

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
