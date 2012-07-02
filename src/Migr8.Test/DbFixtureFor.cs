using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;
using Shouldly;

namespace Migr8.Test
{
    public abstract class DbFixtureFor<TSut> : FixtureFor<TSut>
    {
        static int counter;
        protected string testDatabaseName;
        protected bool dropDatabase = true;

        protected string TestDbConnectionString
        {
            get { return ConnectionString(testDatabaseName); }
        }

        protected string MasterDbConnectionString
        {
            get { return ConnectionString("master"); }
        }

        protected string ConnectionString(string databaseName)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["masterdb"];
            connectionString.ShouldNotBe(null);

            var modifiedConnectionString = connectionString.ConnectionString.Replace("master", databaseName);
            modifiedConnectionString.ShouldContain(databaseName);

            return modifiedConnectionString;
        }

        protected override TSut SetUp()
        {
            SqlConnection.ClearAllPools();

            testDatabaseName = string.Format("migr8_test_{0}", Interlocked.Increment(ref counter));

            Console.WriteLine(@"Test fixture

    {0}

uses test database

    {1}", GetType().Name, testDatabaseName);

            MasterDb(c =>
            {
                Console.WriteLine("Dropping {0}", testDatabaseName);
                c.ExecuteNonQuery("drop database " + testDatabaseName, ignoreException: true);

                Console.WriteLine("Creating {0}", testDatabaseName);
                c.ExecuteNonQuery("create database " + testDatabaseName, ignoreException: true);
            });

            return Create();
        }

        protected override void TearDown()
        {
            if (dropDatabase)
            {
                MasterDb(c =>
                             {
                                 Console.WriteLine("Dropping {0}", testDatabaseName);
                                 c.ExecuteNonQuery("drop database " + testDatabaseName);
                             });
            }
            else
            {
                Console.WriteLine("Database {0} was not dropped!", testDatabaseName);
            }
        }

        protected abstract TSut Create();

        protected void DoNotDropDatabase()
        {
            dropDatabase = false;
        }

        protected void MasterDb(Action<DatabaseContext> masterDbConnectionHandler)
        {
            using (var context = new DatabaseContext(MasterDbConnectionString))
            {
                context.KillConnections(testDatabaseName);

                masterDbConnectionHandler(context);
            }
        }

        protected void TestDb(Action<DatabaseContext> masterDbConnectionHandler)
        {
            using (var context = new DatabaseContext(TestDbConnectionString))
            {
                masterDbConnectionHandler(context);
            }
        }
    }
}