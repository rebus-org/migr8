using System;

namespace Migr8.Npgsql.Test
{
    public static class TestConfig
    {
        public static string PostgresConnectionString => Environment.GetEnvironmentVariable("POSTGRES")
                                                         ?? "host=localhost; db=migr8_test; user id=postgres; password=postgres";
    }
}