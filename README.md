# Migr8

First you install the appropriate Migr8 package - if you're using SQL Server, you will probably

	Install-Package Migr8 -ProjectName YourApp

and if you're using PostgreSQL, you be all

	Install-Package Migr8.Npgsql -ProjectName YourApp

and then you will be good to go :)

## Let's migrate

Execute the migrations - either in a dedicated command line app, or - my favorite - whenever your app starts up,
just before it connects to the database:

```csharp
var connectionString = GetConnectionStringFromSomewhere();

Database.Migrate(connectionString, Migrations.FromThisAssembly());
```

and then, elsewhere in the calling assembly, you define these bad boys (which happen to be valid T-SQL):

```csharp
[Migration(1, "Create table for the Timeout Manager to use")]
class CreateRebusTimeoutsTable : ISqlMigration
{
    public string Sql => @"
		CREATE TABLE [dbo].[RebusTimeouts](
			[id] [int] IDENTITY(1,1) NOT NULL,
			[due_time] [datetimeoffset](3) NOT NULL,
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
```

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

```csharp
[Migration(3, "Table for the first cool thing", branchSpecification: "first-cool-thing")]
class CreateTableForTheFirstCoolThing : ISqlMigration
{
    public string Sql => @"
        CREATE TABLE [dbo].[firstCoolTable] ([id] int)
    "; 
}
```
and
```csharp
[Migration(3, "Table for the next cool thing", branchSpecification: "next-cool-thing")]
class CreateTableForTheNextCoolThing : ISqlMigration
{
    public string Sql => @"
        CREATE TABLE [dbo].[nextCoolTable] ([id] int)
    "; 
}
```
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

```csharp
var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "migrations");

Database.Migrate("db", Migrations.FromFilesIn(dir));
```
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

An SQL file-based migration could look like this:

```sql
-- Create some tables
-- This initial comment will be included as the Description in the migration log

-- This comment is NOT part of the block above and will simply be part of the logged SQL
CREATE TABLE [Table1] ([Id] INT)
GO
-- Same thing with this comment
CREATE TABLE [Table2] ([Id] INT)
```

## Is there more?

One last thing - if you prefer to log things using a logging library, e.g. like the excellent
[Serilog](https://github.com/serilog/serilog), you can make Migr8 output its text to Serilog like this:

```csharp
var connectionString = GetConnectionStringFromSomewhere();
var options = new Options(logAction: text => Log.Information(text));
    
Database.Migrate(connectionString, Migrations.FromAssemblyOf<FirstMigration>(), options);
```

which is probably what you want to do in all of your applications to be sure that Migr8 was properly invoked.
Moreover, if you like, you can change the table that Migr8 uses to store its migration log like this:

```csharp
var connectionString = GetConnectionStringFromSomewhere();
var options = new Options(migrationTableName: "__MilliVanilli");
    
Database.Migrate(connectionString, Migrations.FromAssemblyOf<FirstMigration>(), options);
```

so it doesn't collide with all your other tables named `[__Migr8]`.

## Transactions, locking and such

Migr8 gains exclusive access to the database whenever it wants to execute a migration by starting a
transaction with isolation level SERIALIZABLE, and then it tries to perform an INSERT into the
table that it uses to track migrations.

The inserted row has a migration ID on the form `<number>-<branch-specification>`, which means
that being able to carry out the insert without any conflicts effectively works as taking a named
lock.

After that, the same transaction will be used to execute the migration. If the migration contains
the special `GO` statement (which is an SQL Server Management Studio construction), then each
migration will be executed in its own SQL command, but inside the same transaction.

If you DO NOT want to execute a migration inside a transaction, e.g. if you want to change
the database's recovery mode, you can have the migration executed on its own separate SQL connection 
without a trasaction by specifying the `no-transaction` flag.

If you're using migration classes, you can do it like this by using the `[Hint(...)]` attribute combined
with the predefined hint `Hints.NoTransaction`:

```csharp
[Migration(1, "Prepare for big migration")]
[Hint(Hints.NoTransaction)]
public class SetRecoveryModeSimple : ISqlTransaction
{
	public string Sql => "alter database current set recovery simple";
}
```

If you're using SQL files, you can pass hints to the execution engine by having the text `hints:` as part
of the initial comment of the file, e.g. like this:

```sql
-- Prepare for big migration
-- hints: no-transaction

alter database current set recovery simple
```