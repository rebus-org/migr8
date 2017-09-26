using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NUnit.Framework;
using MySql.Data.MySqlClient;

namespace Migr8.Mysql.Test
{
    public abstract class MysqlFixtureBase
    {
        const int DoesNotExist = 1146;
        readonly ConcurrentStack<IDisposable> _disposables = new ConcurrentStack<IDisposable>();

        [SetUp]
        public void InnerSetUp()
        {
            ResetDatabase();

            SetUp();
        }

        public static void ResetDatabase()
        {
            foreach (var tableName in GetTableNames(false))
            {
                DropTable(tableName);
            }
        }

        static void DropTable(string tableName)
        {
            using (var connection = new MySqlConnection(TestConfig.MysqlConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $@"DROP TABLE `{tableName}`";

                    Console.Write($@"Dropping table ""{tableName}"" - ");
                    try
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("OK");
                    }
                    catch (MySqlException exception) when (exception.ErrorCode == DoesNotExist)
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
            using (var connection = new MySqlConnection(TestConfig.MysqlConnectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $@"SELECT * FROM information_schema.tables WHERE table_schema = '{connection.DataSource}'";

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

        protected void Log(string obj)
        {
            Console.WriteLine($"INFO: {obj}");
        }

        protected void LogVerbose(string obj)
        {
            Console.WriteLine($"VERBOSE: {obj}");
        }
    }
}
