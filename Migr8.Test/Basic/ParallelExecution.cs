using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Migr8.Internals;
using NUnit.Framework;

namespace Migr8.Test.Basic
{
    [TestFixture]
    public class ParallelExecution : FixtureBase
    {
        DatabaseMigratorCore _migrator;

        protected override void SetUp()
        {
            _migrator = new DatabaseMigratorCore(new ThreadPrintingConsoleWriter(), TestConfig.ConnectionString);
        }

        class ThreadPrintingConsoleWriter : IWriter
        {
            public void Write(string text)
            {
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}]: {text}");
            }
        }

        [TestCase(2, 10)]
        [TestCase(4, 20)]
        [TestCase(6, 50)]
        [TestCase(10, 100)]
        public void DoesNotFail(int numberOfThreads, int numberOfMigrations)
        {
            var tablesToCreate = Enumerable.Range(0, numberOfMigrations)
                .Select(n => new
                {
                    Number = n,
                    TableName = $"Table{n:000}"
                })
                .ToList();

            var migrations = tablesToCreate
                .Select(a => new TestMigration(a.Number, "test", $"CREATE TABLE [{a.TableName}] ([Id] INT)"))
                .ToList();

            var threads = Enumerable.Range(0, numberOfThreads)
                .Select(_ => new Thread(() =>
                {
                    _migrator.Execute(migrations);
                }))
                .ToList();

            threads.ForEach(t => t.Start());

            threads.ForEach(t => t.Join(TimeSpan.FromSeconds(20)));

            var tableNames = GetTableNames();

            Assert.That(tableNames, Is.EqualTo(tablesToCreate.Select(a => a.TableName)));
        }
    }
}