# Migr8

	Migrate.Database("server=.;initial catalog=whatever;integrated secutiry=sspi");


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

    [Migration(2, "Create a table for the Timeout Manager to use")]
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