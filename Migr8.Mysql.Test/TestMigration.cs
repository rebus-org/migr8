﻿using System.Collections.Generic;
using System.Linq;
using Migr8.Internals;

namespace Migr8.Mysql.Test
{
    class TestMigration : IExecutableSqlMigration, ISqlMigration
    {
        public TestMigration(int sequenceNumber, string branchSpecification, string sql, string description = null, IEnumerable<string> hints = null)
        {
            SequenceNumber = sequenceNumber;
            BranchSpecification = branchSpecification;
            Id = $"{sequenceNumber}-{branchSpecification}";
            Sql = sql;
            Description = description ?? "";

            SqlMigration = this;
            Hints = hints?.ToList() ?? new List<string>();
        }

        public int SequenceNumber { get; }
        public string BranchSpecification { get; }
        public ISqlMigration SqlMigration { get; }
        public List<string> Hints { get; }
        public string Id { get; }
        public string Sql { get; }
        public string Description { get; }
    }
}