using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    class Hint
    {
        public string Name { get; }
        public string Value { get; }

        public Hint(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public bool HasValue => Value != null;

        public TimeSpan GetValueAsTimeSpan()
        {
            try
            {
                return TimeSpan.Parse(Value);

            }catch(Exception ex)
            {
                throw new FormatException($"Could not parse command timeout. Value is {Value}. Format required: hh:mm:ss", ex);
            }
        }

        public static Hint Parse(string str)
        {
            var index = str.IndexOf(':');
            if (index == -1) return new Hint(str, null);

            var name = str.Substring(0, index);
            var value = index != -1 ? str.Substring(index+1).Trim() : null;
            return new Hint(name, value);
        }
    }
}
