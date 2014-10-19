Chocolatey Explorer
==================
[![Build status](https://ci.appveyor.com/api/projects/status/r3h6ou8b8doydef2)](https://ci.appveyor.com/project/james-dibble/chocolateyexplorer)

A little GUI for reading Chocolatey feeds and installing packages

How to Install
------------------
Chocolatey Explorer is distributed via ClickOnce.  Get your copy [here](http://chocolatey-explorer.jdibble.co.uk/ChocolateyExplorer.exe).  Please raise any bugs [here](https://github.com/james-dibble/ChocolateyExplorer/issues).  The application is currently not signed so Windows will tell you not to trust it.

Changelog
---------
### 0.2.1.100
+ First complete release
+ Default to latest package when selecting one from the tree view
+ Ability to clear the console output

### 0.1.0.99
+ Install package with arguments

### 0.1.0.98
+ Make searching a cancellable operation

### 0.1.0.97
+ Improving searching functionality
+ Bug fixes

### 0.1.0.93
+ Minor performance improvements when loading packages from OData feeds
+ When exploring a feed, packages are loaded in pages.  No more load all as there
  are too many on Chocolatey.
+ Search results can be cleared when completed.
+ Bug fixes

### 0.1.0.61
+ Themed with Modern UI
+ Pacakges can be updated and uninstalled

### 0.1.0.4
+ Add custom source (must be the OData address but can be based upon a Nuget feed)
+ Search a source
+ Load all of the packages from a source
+ View package information

Future Releases
---------------
+ Detect whether Chocolatey is installed.  If not, give user option install it.
+ Better paging functionality; background loading of packages?
+ Update all installed packages
+ Parse and display release notes and descriptions as Markdown
+ Sign the manifests so installer and applicaiton are trusted
+ Authenticated feeds