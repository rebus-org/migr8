# Migr8

Execute the migrations - either in a dedicated command line app, or - my favorite - whenever your app starts up, just before it connects to the database:

	Migrate.Database("server=.;initial catalog=whatever;integrated secutiry=sspi");

and then, elsewhere in the calling assembly, you define these bad boys:

    [Migration(1, "Create table for the Timeout Manager to use")]
    class CreateRebusTimeoutsTable : ISqlMigration
    {
        public string Sql
        {
            get { return @"

                CREATE TABLE [dbo].[timeouts](
	                [time_to_return] [datetime] NOT NULL,
	                [correlation_id] [nvarchar](200) NOT NULL,
	                [saga_id] [uniqueidentifier] NOT NULL,
	                [reply_to] [nvarchar](200) NOT NULL,
	                [custom_data] [nvarchar](max) NULL,
                 CONSTRAINT [PK_timeouts] PRIMARY KEY CLUSTERED 
                (
	                [time_to_return] ASC,
	                [correlation_id] ASC,
	                [reply_to] ASC
                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, 
                    IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, 
                    ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                ) ON [PRIMARY]

				"; 
			}
        }
    }

    [Migration(2, "Create a table for Rebus publishers to use")]
    class CreateRebusSubscriptionsTable : ISqlMigration
    {
        public string Sql
        {
            get { return @"

                CREATE TABLE [dbo].[subscriptions](
	                [message_type] [nvarchar](200) NOT NULL,
	                [endpoint] [nvarchar](200) NOT NULL,
                 CONSTRAINT [PK_subscriptions] PRIMARY KEY CLUSTERED 
                (
	                [message_type] ASC,
	                [endpoint] ASC
                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, 
                    IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, 
                    ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                ) ON [PRIMARY]

				"; 
			}
        }
    }

If you want to be more explicit, you can also go

	// this is how all the migrations are picked up
    var provideMigrations = new AssemblyScanner(typeof(AnEntity).Assembly, typeof(AnotherEntity).Assembly);

	// this one finds out which ones to execute, validates the sequence, and executes
	new DatabaseMigrator(myConnectionString, provideMigrations).MigrateDatabase();

# Brief info

The version number of the database is stored in an extended property called `migr8_database_version`. 

Each migration is executed in a transaction, execution stops if one fails.

Events are raised before/after and in the event of an error.

If you `go` in your SQL script, the preceding block will be executed - just like you're used to, just without the compilation errors if you rename stuff.

# What to do?

First: Use migr8 if you need to treat your database with some evolutionary friction-free schema and data migrations.

Second: _lean back, chill....._