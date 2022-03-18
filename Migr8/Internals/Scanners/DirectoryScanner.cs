using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
// ReSharper disable ArgumentsStyleLiteral

namespace Migr8.Internals.Scanners
{
    class DirectoryScanner
    {
        readonly string _directory;

        public DirectoryScanner(string directory)
        {
            _directory = directory;
        }

        public IEnumerable<IExecutableSqlMigration> GetMigrations()
        {
            if (!Directory.Exists(_directory))
            {
                return Enumerable.Empty<IExecutableSqlMigration>();
            }

            return Directory.GetFiles(_directory, "*.sql", SearchOption.AllDirectories)
                .Where(MathchesMigrationIdPattern)
                .Select(migrationFile => new MigrationFromFile(migrationFile))
                .ToList();
        }

        static bool MathchesMigrationIdPattern(string filePath)
        {
            var migrationId = MigrationId.GetMigrationId(filePath, throwOnError: false);

            return migrationId != null;
        }

        class MigrationId
        {
            public static MigrationId GetMigrationId(string filePath, bool throwOnError = true)
            {
                var extension = Path.GetExtension(filePath);

                if (!string.Equals(extension, ".sql", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                var fileName = Path.GetFileNameWithoutExtension(filePath);

                if (fileName == null)
                {
                    return null;
                }

                var tokens = fileName.Split('-');
                int sequenceNumber;

                if (!int.TryParse(tokens.First(), out sequenceNumber))
                {
                    return null;
                }

                return new MigrationId(sequenceNumber, string.Join("-", tokens.Skip(1)));
            }

            public int SequenceNumber { get; }
            public string BranchSpecification { get; }

            public MigrationId(int sequenceNumber, string branchSpecification)
            {
                SequenceNumber = sequenceNumber;
                BranchSpecification = branchSpecification;
            }

            public string GetPureId()
            {
                return $"{SequenceNumber}-{BranchSpecification}";
            }
        }

        class MigrationFromFile : IExecutableSqlMigration, ISqlMigration
        {
            public MigrationFromFile(string migrationFilePath)
            {
                MigrationFilePath = migrationFilePath;
                var migrationId = MigrationId.GetMigrationId(migrationFilePath);

                Id = migrationId.GetPureId();
                SequenceNumber = migrationId.SequenceNumber;
                BranchSpecification = migrationId.BranchSpecification;

                var lines = File.ReadAllLines(migrationFilePath);

                (Description, Sql) = ExtractDescriptionAndSql(lines);

                SqlMigration = this;

                Hints = ExtractHints(Description);
            }

            List<string> ExtractHints(string description)
            {
                const string hintsPrefix = "hints:";

                var commentLines =
                    description.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

                var hintsLines = commentLines
                    .Select(line => line.Trim())
                    .Where(line => line.StartsWith(hintsPrefix, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                try
                {
                    var hints = hintsLines
                        .SelectMany(line => line.Substring(hintsPrefix.Length).Split(new[] {",", ";"}, StringSplitOptions.RemoveEmptyEntries))
                        .Select(hint => hint.Trim())
                        .Where(hint => !string.IsNullOrWhiteSpace(hint))
                        .Distinct()
                        .OrderBy(hint => hint)
                        .ToList();

                    return hints;
                }
                catch (Exception exception)
                {
                    throw new FormatException($@"An error occurred when extracting hints from the following found hints lines:

{string.Join(Environment.NewLine, hintsLines)}
", exception);
                }
            }

            static (string, string) ExtractDescriptionAndSql(string[] lines)
            {
                bool IsCommentLine(string line) => line.Trim().StartsWith("--");

                var descriptionLines = lines.TakeWhile(IsCommentLine).ToList();
                var sqlLines = lines.SkipWhile(IsCommentLine).ToList();

                return (
                    string.Join(Environment.NewLine, descriptionLines.Select(line => line.Trim().Substring(2).Trim())),
                    string.Join(Environment.NewLine, sqlLines.Where(line => !string.IsNullOrWhiteSpace(line)))
                );

                //var firstLineIsComment = lines.FirstOrDefault()?.StartsWith("--") ?? false;
                //var parsingDescription = firstLineIsComment;
                //var commentLines = new List<string>();
                //var sqlLines = new List<string>();

                //foreach (var line in lines)
                //{
                //    var trimmedLine = line.Trim();
                //    if (string.IsNullOrWhiteSpace(trimmedLine))
                //    {
                //        if (commentLines.Any())
                //        {
                //            parsingDescription = false;
                //            continue;
                //        }

                //        continue;
                //    }

                //    if (parsingDescription)
                //    {
                //        if (trimmedLine.StartsWith("--"))
                //        {
                //            commentLines.Add(trimmedLine);
                //        }
                //    }
                //    else
                //    {
                //        sqlLines.Add(line);
                //    }
                //}

                //var description = string.Join(Environment.NewLine, commentLines
                //    .Where(line => !string.IsNullOrWhiteSpace(line))
                //    .Select(line => line.TrimStart(' ', '-')));

                //var sql = string.Join(Environment.NewLine, sqlLines);

                //return (description, sql);
            }

            static string ExtractDescription(string[] lines)
            {
                var commentLines = new List<string>();

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedLine))
                    {
                        if (commentLines.Any()) break;
                        continue;
                    }

                    if (trimmedLine.StartsWith("--"))
                    {
                        commentLines.Add(trimmedLine);
                    }
                }

                return string.Join(Environment.NewLine,
                    commentLines
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(line => line.TrimStart(' ', '-')));
            }

            static string ExtractMigration(IEnumerable<string> lines)
            {
                var sqlLines = lines.SkipWhile(IsPartOfComments);

                return string.Join(Environment.NewLine, sqlLines);
            }

            static bool IsPartOfComments(string line)
            {
                return string.IsNullOrWhiteSpace(line)
                       || line.TrimStart().StartsWith("--");
            }

            public string Id { get; }
            public string Sql { get; }
            public string Description { get; }
            public int SequenceNumber { get; }
            public string BranchSpecification { get; }
            public ISqlMigration SqlMigration { get; }
            public List<string> Hints { get; }
            public string MigrationFilePath { get; }
        }
    }
}