using Migr8.Internals;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Migr8.Mysql.Test.hints.scripts
{
    [TestFixture]
    class TestHintParsing
    {
        [Test]
        public void TryParsingHints()
        {

            var input = new HashSet<string> { "sql-command-timeout: 30s"};

            var hints = HintParser.ParseHints(input);

            Hint hint = hints.GetHint("sql-command-timeout");

            Assert.That(hint, Is.Not.Null);
            Assert.That(hint.Value, Is.EqualTo("30s"));

        }

        [Test]
        public void CanParseValueAsTimeSpan()
        {
            var hint = new Hint("sql-command-timeout", "00:00:30");

            var timeSpan = hint.GetValueAsTimeSpan();

            Assert.That(timeSpan, Is.EqualTo(TimeSpan.FromSeconds(30)));
        }

        [Test]
        public void CanParseHintWithColonInValue()
        {
            var input = new HashSet<string> { "sql-command-timeout: 00:00:30" };

            var hints = HintParser.ParseHints(input);

            Assert.That(hints.Count, Is.EqualTo(1));
            Assert.That(hints.First().Value, Is.EqualTo("00:00:30"));
        }
    }
}
