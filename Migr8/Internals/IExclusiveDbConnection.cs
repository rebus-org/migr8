using System;
using System.Collections.Generic;

namespace Migr8.Internals
{
    interface IExclusiveDbConnection : IDisposable
    {
        void Complete();
        HashSet<string> GetTableNames();
        void LogMigration(IExecutableSqlMigration migration, string migrationTableName);
        void CreateMigrationTable(string migrationTableName);
        void ExecuteStatement(string sqlStatement, TimeSpan? sqlCommandTimeout = null);
        IEnumerable<string> GetExecutedMigrationIds(string migrationTableName);
    }
}