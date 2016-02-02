# Migr8

First you install the appropriate Migr8 package - if you're using SQL Server, you will probably

	Install-Package Migr8 -ProjectName YourApp

and if you're using PostgreSQL, you be all

	Install-Package Migr8.Npgsql -ProjectName YourApp

and then you will be good to go :)

## Let's migrate

Execute the migrations - either in a dedicated command line app, or - my favorite - whenever your app starts up,
just before it connects to the database:

	Database.Migrate("db", Migrations.FromThisAssembly());

where `db` is a key in the `connectionStrings` section of your app.config/web.config, and then, elsewhere in the
calling assembly, you define these bad boys (which happen to be valid T-SQL):

    [Migration(1, "Create table for the Timeout Manager to use")]
    class CreateRebusTimeoutsTable : ISqlMigration
    {
        public string Sql => @"
            CREATE TABLE [dbo].[RebusTimeouts](
                [id] [int] IDENTITY(1,1) NOT NULL,
	            [due_time] [datetime2](7) NOT NULL,
	            [headers] [nvarchar](MAX) NOT NULL,
	            [body] [varbinary](MAX) NOT NULL,
                CONSTRAINT [PK_RebusTimeouts] PRIMARY KEY NONCLUSTERED 
                (
	                [id] ASC
                )
            )
		"; 
    }

    [Migration(2, "Create a table for Rebus publishers to use")]
    class CreateRebusSubscriptionsTable : ISqlMigration
    {
        public string Sql => @"
            CREATE TABLE [dbo].[RebusSubscriptions] (
	            [topic] [nvarchar](200) NOT NULL,
	            [address] [nvarchar](200) NOT NULL,
                CONSTRAINT [PK_RebusSubscriptions] PRIMARY KEY CLUSTERED 
                (
	                [topic] ASC,
	                [address] ASC
                )
            )
		"; 
    }

In the example above, I've created two migrations which will be executed in they order indicated by their
 _sequence number_.

## Branch specifications

Since you will most likely not be the only one developing things in your application, you might have adopted
a _git flow_-inspired branching model, where each developer will create a branch to work in.

For example, two developers working in the `feature/first-cool-thing` and `feature/next-cool-thing` branches
might need to create the next migration. In the old days, one of them would be the unfortunate one to last
integrate _migration 3_ back into master, it would be necessary to change the migration to be number 4 and
probably execute the other developer's migration manually.

Luckily, they chose to use Migr8 to evolve their database, so they just go ahead and create

    [Migration(3, "Table for the first cool thing", branchSpecification: "first-cool-thing")]
    class CreateTableForTheFirstCoolThing : ISqlMigration
    {
        public string Sql => @"
            CREATE TABLE [dbo].[firstCoolTable] ([id] int)
        "; 
    }

and

    [Migration(3, "Table for the next cool thing", branchSpecification: "next-cool-thing")]
    class CreateTableForTheNextCoolThing : ISqlMigration
    {
        public string Sql => @"
            CREATE TABLE [dbo].[nextCoolTable] ([id] int)
        "; 
    }

which will NOT BE A PROBLEM AT ALL, because they remembered to set the `branchSpecification` to the names
of their branches.

Migr8 will use the branch specifications to keep track of which migrations it has executed in the database,
and it will then ensure that all migrations are applied eventually.

Just remember that the sequence number positions a migration in a global sequence, which then effectively
functions as a way to specify on which version of the already-existing schema each migration depends.

## More information

By default, the table `[__Migr8]` is used to track extensive information on applied migrations - including
the full SQL that was executed + more.

Each migration is executed in a SERIALIZABLE transaction - execution stops if one fails.

If you `go` in your SQL script, the preceding block will be executed - just like you're used to when working
in SQL Server Management Studio.

The migrator competes perfectly for getting to apply the next migration, which guarantees no surprises even
if you run the migrator at startup in your multi-instance Azure Website, or on a web farm.

## What to do?

First: Use Migr8 if you need to treat your database with some evolutionary friction-free schema and data
migrations.

Second: _lean back, chill....._

## What else can you do?

You can also

    var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "migrations");

    Database.Migrate("db", Migrations.FromFilesIn(dir));

in order to pick up migrations from files named on the form `"<sequence-number>-<branch-specification>.sql"`,
e.g. organized like this:

    /v01_00_00
        0001-master.sql

        /feature-a
            0002-feature-a.sql
            0003-feature-a.sql
            0004-feature-a.sql
        
        /feature-b
            0002-feature-b.sql
            0003-feature-b.sql
   
    /v01_01_00
        0005-master.sql     

allowing you to keep track of a huge number of migrations, probably only needing to move a few of them around
whenever you integrate a feature branch with master. Comments added at the beginning of the `.sql` file will be
treated as the migration's description.

## Is there more?

One last thing - if you prefer to log things using a logging library, e.g. like the excellent
[Serilog](https://github.com/serilog/serilog), you can make Migr8 output its text to Serilog like this:

    var options = new Options(logAction: text => Log.Information(text));
    
    Database.Migrate("db", Migrations.FromAssemblyOf<FirstMigration>(), options);

which is probably what you want to do in all of your applications to be sure that Migr8 was properly invoked.
Moreover, if you like, you can change the table that Migr8 uses to store its migration log like this:

    var options = new Options(migrationTableName: "__MilliVanilli");
    
    Database.Migrate("db", Migrations.FromAssemblyOf<FirstMigration>(), options);

so it doesn't collide with all your other tables named `[__Migr8]`.