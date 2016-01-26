namespace Migr8.Test
{
    public static class TestConfig
    {
        public static string ConnectionString => "server=.; database=migr8_test; trusted_connection=true";

        public static string PostgresConnectionString => "host=localhost; db=migr8_test; user id=postgres; password=postgres";
    }
}