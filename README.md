Chocolatey Explorer
==================
A little GUI for reading Chocolatey feeds and installing packages
[![Build status](https://ci.appveyor.com/api/projects/status/r3h6ou8b8doydef2)](https://ci.appveyor.com/project/james-dibble/chocolateyexplorer)

How to Install
------------------
Chocolatey Explorer is distributed via ClickOnce.  Get your copy [here](http://chocolatey-explorer.jdibble.co.uk/ChocolateyExplorer.application).  It's in very early beta stages so expect many many bugs, so if you find one please raise them [here](https://github.com/james-dibble/ChocolateyExplorer/issues).  The application is currently not signed so Windows will tell you not to trust it.

Changelog
---------
* 0.1.0.93
    + Minor performance improvements when loading packages from OData feeds
    + When exploring a feed, packages are loaded in pages.  No more load all as there
      are too many on Chocolatey.
    + Search results can be cleared when completed.
    + Bug fixes
* 0.1.0.61
    + Themed with Modern UI
    + Pacakges can be updated and uninstalled
* 0.1.0.4
    + Add custom source (must be the OData address but can be based upon a Nuget feed)
    + Search a source
    + Load all of the packages from a source
    + View package information
    + Install a Chocolatey package