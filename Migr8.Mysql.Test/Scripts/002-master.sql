-- This is a test
-- hints: sql-command-timeout: 00:00:30; no-transaction

CREATE TABLE `Bimse2` (
	`Id` BIGINT NOT NULL AUTO_INCREMENT,
	`Data` TEXT,
	PRIMARY KEY (`Id`)
);

DROP TABLE `Bimse2`;