using System;
using System.Collections.Generic;
using System.Linq;

namespace Migr8.Internals
{
    static class HintParser
    {
        public static List<Hint> ParseHints(IEnumerable<string> hints)
        {
            return hints.Select(Hint.Parse).ToList();
        }

        public static Hint GetHint(this IEnumerable<Hint> hints, string name)
        {
            return hints.SingleOrDefault(hint => hint.Name == name);
        }
    }

    class Hint(string name, string value)
    {
        public string Name { get; } = name;
        public string Value { get; } = value;

        public bool HasValue => Value != null;

        public TimeSpan GetValueAsTimeSpan()
        {
            try
            {
                return TimeSpan.Parse(Value);

            }
            catch (Exception ex)
            {
                throw new FormatException($"Could not parse command timeout. Value is {Value}. Format required: hh:mm:ss", ex);
            }
        }

        public static Hint Parse(string str)
        {
            var parts = str.Split(':').Select(p => p.Trim()).ToList();
            
            return new Hint(parts.First(), string.Join(":", parts.Skip(1)));
        }
    }
}
