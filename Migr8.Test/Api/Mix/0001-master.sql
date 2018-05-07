-- This is my first migration
-- A table is created
--
-- This comment SHOULD be included, because it's part of the first comment block
--
-- hints: hint1, hint2,    hint3;   hint4
-- hints: hint5, hint-6

-- This comment should NOT be included, because it's not considered connected to the first comment block

-- Create a table
create table [Tabelle1]
(
	[Id] int identity(1,1)
)

go

-- Add a column to that table
alter table [Tabelle1] add [Text] nvarchar(10);