-- This is my first migration
--
-- hints: sql-command-timeout: 00:01:00

create table [Tabelle1]
(
	[Id] int identity(1,1)
)

go

WAITFOR DELAY '00:00:05';