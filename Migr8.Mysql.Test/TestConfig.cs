using System;

namespace Migr8.Mysql.Test
{
    public static class TestConfig
    {
        public static string MysqlConnectionString => Environment.GetEnvironmentVariable("MYSQL")
                                                         ?? "server=localhost;uid=root;pwd=root;database=test";
    }
}