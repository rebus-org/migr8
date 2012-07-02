using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Migr8
{
    internal static class DbConnectionExtensions
    {
        public static Dictionary<string, string> DatabaseProperties(this DatabaseContext context)
        {
            var properties = context.ExecuteQuery(@"select * from sys.extended_properties where [class] = 0 and CLASS_DESC = 'DATABASE'");

            return properties.ToDictionary(r => r["name"].ToString(), r => r["value"].ToString());
        }

        public static List<string> TableNames(this DatabaseContext context)
        {
            var tables = context.ExecuteQuery("select * from sys.Tables");

            return tables.Select(r => r["name"].ToString()).ToList();
        }
    }
}