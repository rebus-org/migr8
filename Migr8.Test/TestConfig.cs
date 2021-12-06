using System;

namespace Migr8.Test
{
    public static class TestConfig
    {
        public static string ConnectionString => Environment.GetEnvironmentVariable("SQLSERVER")
                                                 ?? "server=.; database=migr8_test; trusted_connection=true; encrypt=false";
    }
}