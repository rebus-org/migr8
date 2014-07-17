using System.Collections.Generic;
using System.Linq;

namespace Migr8.DB
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

        public static T GetSingleValue<T>(this DatabaseContext context, string tableName, string valueName)
        {
            var property = context.ExecuteQuery(string.Format("SELECT * FROM dbo.{0}", tableName)).Single();
            return (T) property[valueName];
        }
    }
}