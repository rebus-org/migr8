using System.Linq;

namespace Migr8.DB
{
    public class ExtendedPropertiesPersister : IVersionPersister
    {
        public void EnsureSchema(DatabaseContext context)
        {
            var sql = string.Format("select * from sys.extended_properties where [class] = 0 and [name] = '{0}'", ExtProp.DatabaseVersion);

            var properties = context.ExecuteQuery(sql);

            if (properties.Count == 0)
            {
                context.ExecuteNonQuery(string.Format("exec sys.sp_addextendedproperty @name=N'{0}', @value=N'{1}'", ExtProp.DatabaseVersion, "0"));
            }
        }

        public int GetDatabaseVersionNumber(DatabaseContext context)
        {
            var versionProperty = context.ExecuteQuery(string.Format("select * from sys.extended_properties where [class] = 0 and [name] = '{0}'", ExtProp.DatabaseVersion))
                .Single();
            var currentVersion = int.Parse(versionProperty["value"].ToString());
            return currentVersion;
        }

        public void UpdateVersion(DatabaseContext context, int newVersion)
        {
            context.ExecuteNonQuery(string.Format("exec sys.sp_updateextendedproperty @name=N'{0}', @value=N'{1}'", ExtProp.DatabaseVersion, newVersion));
        }
    }
}