using Migr8.SqlServer;
using NUnit.Framework;

namespace Migr8.Test.Basic
{
    [TestFixture]
    public class ConnectUsingManagedIdentity
    {
        [Test]
        [Ignore("meant to be executed manually and F5-debugged :)")]
        public void TryGettingConnection()
        {
            //var realisticConnectionString = "server=tcp:azure-wjateverino-mssql10.database.windows.net,1433; Database=some-database; Authentication=Active Directory Interactive";
            var realisticConnectionString = "server=tcp:azur-dva-sql01.database.windows.net,1433;Database=GUPower; Authentication=Active Directory Interactive";

            var sqlServerDb = new SqlServerDb();
            
            var exclusiveDbConnection = sqlServerDb.GetExclusiveDbConnection(realisticConnectionString, new Options(), new ThreadPrintingConsoleWriter());
        }
    }
}