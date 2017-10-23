using Migr8.Internals;

namespace Migr8.Mysql.Mysql
{
    class MysqlDb : IDb
    {
        public IExclusiveDbConnection GetExclusiveDbConnection(string connectionString, Options options)
        {
            return new MysqlDbExclusiveDbConnection(connectionString, options);
        }
    }
}