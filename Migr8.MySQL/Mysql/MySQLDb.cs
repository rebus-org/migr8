using Migr8.Internals;

namespace Migr8.Mysql.Mysql
{
    class MysqlDb : IDb
    {
        public IExclusiveDbConnection GetExclusiveDbConnection(string connectionString, Options options, IWriter writer, bool useTransaction = true)
        {
            return new MysqlDbExclusiveDbConnection(connectionString, options, useTransaction);
        }
    }
}