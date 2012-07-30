using System;
using System.Collections.Generic;
using FakeItEasy;
using NUnit.Framework;
using Shouldly;

namespace Migr8.Test.DatabaseMigrator
{
    [TestFixture]
    public class when_executing_migrations : DbFixtureFor<Migr8.DatabaseMigrator>
    {
        #region oh my god it's not pretty
        const string RealisticSqlSnippet = @"

CREATE TABLE [dbo].[Acknowledgements](
	[AcknowledgementID] [int] IDENTITY(1,1) NOT NULL,
	[ReasonCode] [nvarchar](5) NULL,
	[ReasonText] [nvarchar](250) NOT NULL,
	[DocumentID] [int] NOT NULL,
	[DocumentTypeID] [int] NOT NULL,
	[ReceivingDocumentID] [int] NULL,
 CONSTRAINT [PK_Acknowledgements] PRIMARY KEY CLUSTERED 
(
	[AcknowledgementID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [uc_AcknownledgeDocumentID] UNIQUE NONCLUSTERED 
(
	[DocumentID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

";
        #endregion

        IProvideMigrations provideMigrations;
        List<string> raisedEvents;

        protected override Migr8.DatabaseMigrator Create()
        {
            raisedEvents = new List<string>();
            provideMigrations = A.Fake<IProvideMigrations>();
            var migrator = new Migr8.DatabaseMigrator(TestDbConnectionString, provideMigrations);
            migrator.BeforeExecute += BeforeExecute;
            migrator.AfterExecuteSuccess += AfterExecuteSuccess;
            migrator.AfterExecuteError += AfterExecuteError;
            return migrator;
        }

        void BeforeExecute(IMigration migration)
        {
            raisedEvents.Add(string.Format("BEFORE {0}", migration.TargetDatabaseVersion));
        }

        void AfterExecuteSuccess(IMigration migration)
        {
            raisedEvents.Add(string.Format("AFTER {0}", migration.TargetDatabaseVersion));
        }

        void AfterExecuteError(IMigration migration, Exception exception)
        {
            raisedEvents.Add(string.Format("AFTER {0} (ERROR)", migration.TargetDatabaseVersion));
        }

        [Test]
        public void database_version_number_is_incremented()
        {
            // arrange
            A.CallTo(() => provideMigrations.GetAllMigrations())
                .Returns(new[]
                             {
                                 NewMigration(1, RealisticSqlSnippet),
                                 NewMigration(2, "drop table Acknowledgements"),
                                 NewMigration(3, RealisticSqlSnippet),
                                 NewMigration(4, "drop table Acknowledgements"),
                             });

            // act
            sut.MigrateDatabase();

            // assert
            TestDb(db =>
                       {
                           db.TableNames().Count.ShouldBe(0);
                           db.DatabaseProperties().ShouldContainKey(Constants.DatabaseVersionPropertyName);
                           db.DatabaseProperties()[Constants.DatabaseVersionPropertyName].ShouldBe("4");
                       });
        }

        [Test]
        public void only_current_and_future_migrations_are_executed()
        {
            // arrange
            TestDb(db => db.ExecuteNonQuery(string.Format("exec sys.sp_addextendedproperty @name=N'{0}', @value=N'3'", Constants.DatabaseVersionPropertyName)));

            A.CallTo(() => provideMigrations.GetAllMigrations())
                .Returns(new[]
                             {
                                 NewMigration(1, "!!!!will generate an error if executed!!!!!"),
                                 NewMigration(2, "!!!!will generate an error if executed!!!!!"),
                                 NewMigration(3, "!!!!will generate an error if executed!!!!!"),
                                 NewMigration(4, "create table test1 (id int identity(1,1) not null)"), //< only this one should be executed
                             });

            // act
            sut.MigrateDatabase();

            // assert
            TestDb(db =>
                       {
                           db.TableNames().Count.ShouldBe(1);
                           db.TableNames().ShouldContain("test1");
                       });
        }

        [Test]
        public void migrations_are_executed_in_order()
        {
            // arrange
            A.CallTo(() => provideMigrations.GetAllMigrations())
                .Returns(new[]
                             {
                                 NewMigration(3, "create table test1 (id int identity(1,1) not null)"),
                                 NewMigration(2, "drop table Acknowledgements"),
                                 NewMigration(1, RealisticSqlSnippet),
                             });

            // act
            sut.MigrateDatabase();

            // assert
            TestDb(db =>
                       {
                           db.TableNames().Count.ShouldBe(1);
                           db.TableNames().ShouldContain("test1");
                       });
        }

        [Test]
        public void before_and_after_events_are_raised()
        {
            // arrange
            A.CallTo(() => provideMigrations.GetAllMigrations())
                .Returns(new[]
                             {
                                 NewMigration(3, "will throw!!!"),
                                 NewMigration(2, "--"),
                                 NewMigration(1, "--"),
                             });


            // act
            try{ sut.MigrateDatabase();}
            catch{}

            // assert
            string.Join(",", raisedEvents).ShouldBe("BEFORE 1,AFTER 1,BEFORE 2,AFTER 2,BEFORE 3,AFTER 3 (ERROR)");
        }

        IMigration NewMigration(int targetDatabaseVersion, string sql)
        {
            var migration = A.Fake<IMigration>();
            A.CallTo(() => migration.TargetDatabaseVersion).Returns(targetDatabaseVersion);
            A.CallTo(() => migration.SqlStatements).Returns(new[] { sql });
            return migration;
        }
    }
}