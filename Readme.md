**Build Status**

| Windows                 |  Linux/Mac              | Code Coverage         | NuGet                 | NuGet (Core)          |
| ------------------------|-------------------------|-----------------------|-----------------------|-----------------------|
| [![Build status][1]][2] | [![Build Status][3]][4] | [![codecov][5]][6]    | [![NuGet][7]][8]      | [![NuGet][9]][10]      |

[1]: https://ci.appveyor.com/api/projects/status/7oo0gyq4fv0jvsjn/branch/master?svg=true
[2]: https://ci.appveyor.com/project/robotdotnet/networktables/branch/master
[3]: https://travis-ci.org/robotdotnet/NetworkTables.svg?branch=master
[4]: https://travis-ci.org/robotdotnet/NetworkTables
[5]: https://codecov.io/gh/robotdotnet/NetworkTables/branch/master/graph/badge.svg
[6]: https://codecov.io/gh/robotdotnet/NetworkTables
[7]: https://img.shields.io/nuget/v/FRC.NetworkTables.svg
[8]: https://www.nuget.org/packages/FRC.NetworkTables
[9]: https://img.shields.io/nuget/vpre/FRC.NetworkTables.Core.svg
[10]: https://www.nuget.org/packages/FRC.NetworkTables.Core

NetworkTables is a DotNet implementation of the NetworkTables protocol commonly used in FRC. Currently implements v3 of the NetworkTables spec.

This repository contains two seperate release projects. 

The first is NetworkTables, which is a complete port of the ntcore library from C++ to DotNet. This library is recommended for any clients that you wish to create, as the dependancies are very low, and supported by most platforms.

The second project is NetworkTables.Core. This is a wrapper around the official ntcore library. This means that the networking code has been tested more by the community, and is recommended for running on an FRC robot as the server. 



Supported Platforms - NetworkTables
-----------------------------------
* All systems that support the frameworks listed below
* .NET 4.5.1 or higher
* .NET Standard 1.3 or higher:
  * System.Net.NameResolution
  * System.ComponentModel.EventBasedAsync

Supported Platforms - NetworkTables.Core
----------------------------------------
* .NET 4.5.1 or higher
* .NET Standard 1.5
* Since this uses a native library, only the platforms listed below are supported
  * Windows x86 and amd64
  * Linux x86 and amd64
  * Mac OS x86 and x86-64
  * RoboRio (Soft Float Arm v7)

Installation
------------
When you create a WPILib robot project using our VisualStudio extension, NetworkTables.Core will automatically be installed.

For new installs, see the badges at the top for NuGet packages.

Note that Xamarin requires 4.1.2 minimum in order to work directly from NuGet. If you require support for older versions, 
you need to download and manually use the .NET 4.5.1 library, as Xamarin by default will attempt to use the Net Standard build and then error on build. 
Note that only Android has been tested, but iOS should work without issue.


License
=======
See [LICENSE.txt](LICENSE.txt)


Contributors
============

Thad House (@thadhouse)

Peter Johnson (@peterjohnson) wrote the original ntcore library, which was heavily used to port to native
