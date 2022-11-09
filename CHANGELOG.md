# Changelog

## 0.9.0-alpha
* threw out the old Migr8 and remade it

## 0.9.0-beta
* fixed some stuff

## 0.9.1-alpha 
* fixed more stuff

## 0.9.2-alpha
* even more stuff fixed

## 0.9.3
* actually works

## 0.9.4
* added validation that prevents accidentally executing migrations, that come before those that have already been executed

## 0.9.5
* added ability to pick up `Migrations.FromFilesIn(aDirectory)` or just `Migrations.FromFilesInCurrentDirectory()`

## 0.13.0
* increased SQL migration command timeout to 10 minutes
* added PostgreSQL support (just use the `Migr8.Npgsql` package instead)

## 0.14.0
* better exceptions on errors during type scan - thanks [madstt]

## 0.21.0
* Target .NET Standard 2.0, because that's a good sane target
* Add SQL command timeout option
* Better comment parsing when using .sql files
* Add `.ToList()` method on `Migrations` to allow for tooling to inspect found migrations
* Add support for "hints", i.e. passing special tags along with migrations to instruct the execution engine to do certain things. For now, `hints: no-transaction` / `[Hint(Hints.NoTransaction)]` will cause the migration to be executed on its own SQL connection without a transaction
* Update MySQL driver to 8.0.17
* Update Postgres driver to 4.0.10
* Remove support for `System.Configuration.ConfigurationManager`
* Support Azure Managed Identity authentication by setting `Authentication=ManagedIdentity` or `Authentication=Active Directory Interactive` in the connection string
* Fix bug in MySQL implementation that would return incorrect table names when querying the database for them

## 0.22.0
* Make managed identity token retrieval use the hostname from the connection string to obtain the token

## 0.23.0
* Add ability to configure SQL command timeout individually for a script - thanks [ctrlenter]

## 0.24.0
* Detect `Authentication=Active Directory Integrated` the same way as `Authentication=Active Directory Interactive`

## 0.25.0
* Use Microsoft.Data.SqlClient instead

## 0.26.0
* Extend accepted Microsoft.Data.SqlClient version range

## 0.27.0
* Extend accepted Microsoft.Data.SqlClient version range

## 0.28.0
* Fix bug that would result in not picking up migrations from files when there was no description

## 0.29.0
* Update all the packages

---

[ctrlenter]: https://github.com/ctrlenter
[madstt]: https://github.com/madstt