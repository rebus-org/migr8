using Migr8.Internals;

namespace Migr8.Mysql.Mysql
{
    class MysqlDb : IDb
    {
        public IExclusiveDbConnection GetExclusiveDbConnection(string connectionString)
        {
            return new MysqlDbExclusiveDbConnection(connectionString);
        }
    }
}