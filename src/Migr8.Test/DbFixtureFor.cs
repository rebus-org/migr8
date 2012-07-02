using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using NUnit.Framework;

namespace Migr8.Test
{
    public abstract class DbFixtureFor<TSut> : FixtureFor<TSut>
    {
        static int counter = 0;
        protected string testDatabaseName;

        protected string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["masterdb"].ConnectionString; }
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            testDatabaseName = String.Format("migr8_test_{0}", Interlocked.Increment(ref counter));
            Console.WriteLine(@"Test fixture

    {0}

uses test database

    {1}", GetType().Name, testDatabaseName);

            MasterDb(c => c.ExecuteNonQuery("drop database " + testDatabaseName, ignoreException: true));
            MasterDb(c => c.ExecuteNonQuery("create database " + testDatabaseName, ignoreException: true));
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            MasterDb(c => c.ExecuteNonQuery("drop database " + testDatabaseName));
        }

        protected void MasterDb(Action<DatabaseContext> masterDbConnectionHandler)
        {
            using (var context = new DatabaseContext(ConnectionString))
            {
                masterDbConnectionHandler(context);
            }
        }

        protected internal void TestDb(Action<DatabaseContext> masterDbConnectionHandler)
        {
            using (var context = new DatabaseContext(GetTestDbConnection()))
            {
                masterDbConnectionHandler(context);
            }
        }

        protected IDbConnection GetTestDbConnection()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["masterdb"].ConnectionString;

            var connection = new SqlConnection(connectionString);
            connection.Open();

            return connection;
        }
    }
}