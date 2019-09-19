using System;
using System.Collections.Generic;
using System.Linq;

namespace Migr8.Internals
{
    class ConnectionStringMutator
    {
        readonly List<KeyValuePair<string, string>> _pairs;

        public ConnectionStringMutator(string connectionString)
        {
            _pairs = connectionString.Split(';')
                .Select(pair => pair.Trim())
                .Select(pair => pair.Split('='))
                .Select(parts => new KeyValuePair<string, string>(parts.First().Trim(), parts.LastOrDefault()?.Trim() ?? ""))
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Key))
                .ToList();
        }

        public ConnectionStringMutator Without(Func<KeyValuePair<string, string>, bool> elementMatcher)
        {
            var pairsToRemove = _pairs.Where(elementMatcher).ToList();

            return new ConnectionStringMutator(_pairs.Except(pairsToRemove));
        }

        public override string ToString()
        {
            return string.Join("; ", _pairs.Select(p => $"{p.Key}={p.Value}"));
        }

        ConnectionStringMutator(IEnumerable<KeyValuePair<string, string>> pairs) => _pairs = pairs.ToList();

        public bool HasElement(string key, string value = null, StringComparison comparison = StringComparison.CurrentCulture)
        {
            return _pairs.Any(p => string.Equals(p.Key, key, comparison)
                                   && string.Equals(p.Value, value ?? "", comparison));
        }
    }
}