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

## 0.20.0-b1

* .NET Core support (targeting .NET 4.5 and .NET Standard 1.6)

---

[madstt]: https://github.com/madstt