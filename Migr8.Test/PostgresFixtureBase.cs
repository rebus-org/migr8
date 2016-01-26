using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using Npgsql;
using NUnit.Framework;

namespace Migr8.Test
{
    public abstract class PostgresFixtureBase
    {
        const string DoesNotExist = "42P01";
        readonly ConcurrentStack<IDisposable> _disposables = new ConcurrentStack<IDisposable>();

        [SetUp]
        public void InnerSetUp()
        {
            ResetDatabase();

            SetUp();
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
            using (var connection = new NpgsqlConnection(TestConfig.PostgresConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $@"DROP TABLE ""{tableName}""";

                    Console.Write($@"Dropping table ""{tableName}"" - ");
                    try
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("OK");
                    }
                    catch (NpgsqlException exception) when (exception.Code == DoesNotExist)
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
            using (var connection = new NpgsqlConnection(TestConfig.PostgresConnectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select * from information_schema.tables where table_schema not in ('pg_catalog', 'information_schema')";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(reader["table_name"].ToString());
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
