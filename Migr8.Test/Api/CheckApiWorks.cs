﻿using System;
using NUnit.Framework;

namespace Migr8.Test.Api
{
    [TestFixture]
    public class CheckApiWorks : FixtureBase
    {
        [Test]
        public void ItWorks()
        {
            Database.Migrate(TestConfig.ConnectionString, Migrations.FromAssemblyOf<CheckApiWorks>(), new Options(logAction: text => Console.WriteLine("LOG: {0}", text)));

            var tableNames = GetTableNames();

            Assert.That(tableNames, Is.EqualTo(new[] {"MyFirstTable", "MySecondTable"}));
        }
    }
}