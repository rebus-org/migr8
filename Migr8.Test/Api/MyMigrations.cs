namespace Migr8.Test.Api
{
    [Migration(1, "This is my first migration", "master")]
    class CreateMyFirstTable : ISqlMigration
    {
        public string Sql => @"CREATE TABLE [MyFirstTable] ([Id] int)";
    }

    [Migration(2, "This is my second migration", "feature-1")]
    class CreateMySecondTable : ISqlMigration
    {
        public string Sql => @"CREATE TABLE [MySecondTable] ([Id] int)";
    }

    [Migration(2, "This is my second migration", "feature-2")]
    class AddColumnToMyFirstTable : ISqlMigration
    {
        public string Sql => @"ALTER TABLE [MyFirstTable] ADD [Text] NVARCHAR(100) NULL";
    }

    [Migration(3, "This is my third migration", "master")]
    class AddColumnsToBothTables : ISqlMigration
    {
        public string Sql => @"

ALTER TABLE [MyFirstTable] ADD [MoreText] NVARCHAR(100) NULL

GO

ALTER TABLE [MySecondTable] ADD [MoreText] NVARCHAR(100) NULL

";
    }

    [Migration(4, "Change recovery mode to simple")]
    [Hint("no-transaction")]
    class ChangeRecoveryMode : ISqlMigration
    {
        public string Sql => @"ALTER DATABASE CURRENT SET RECOVERY SIMPLE";
    }

    [Migration(5, "Change recovery mode back to full")]
    [Hint(Hints.NoTransaction)]
    class ChangeRecoveryModeBack : ISqlMigration
    {
        public string Sql => @"ALTER DATABASE CURRENT SET RECOVERY FULL";
    }
}