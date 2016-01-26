using System;
using System.Collections.Generic;

namespace Migr8.Internals
{
    interface IExclusiveDbConnection : IDisposable
    {
        void Complete();
        HashSet<string> GetTableNames();
        IEnumerable<T> Select<T>(string columnName, string query);
        void LogMigration(IExecutableSqlMigration migration, string migrationTableName);
        void CreateMigrationTable(string migrationTableName);
        void ExecuteStatement(string sqlStatement);
    }
}