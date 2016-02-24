# Changelog

* `0.9.0-alpha` - threw out the old Migr8 and remade it
* `0.9.0-beta` - fixed some stuff
* `0.9.1-alpha` - fixed more stuff
* `0.9.2-alpha` - even more stuff fixed
* `0.9.3` - actually works
* `0.9.4` - added validation that prevents accidentally executing migrations, that come before those that have already been executed
* `0.9.5` - added ability to pick up `Migrations.FromFilesIn(aDirectory)` or just `Migrations.FromFilesInCurrentDirectory()`
* `0.9.6` - increased SQL migration command timeout to 10 minutes
