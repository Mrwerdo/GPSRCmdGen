RoboCup@Home Command Generator
==============================

*Note: this is a fork of the original repository which contains modifications for simulaneously generating commands and sentences.*
*It does not include QR code generation or a GUI.*

------------------------------

Set of sentence generators for the RoboCup @Home tasks.
The latest updates follows following the tasks, rules, and specifications of the Rulebook for Sydney 2019.

To learn more about the rules, visit the [RoboCup@Home website](http://athome.robocup.org) and the [Rulebook repository](https://github.com/RoboCupAtHome/RuleBook/).

The solution includes generators for the following tasks:
- General Purpose Service Robot (GPSR)
- Enhanced General Purpose Service Robot (EGPSR)


## Minimum System Requirements

You need [Microsoft .NET Core 3.1][dotnet-core] or above.

Although the code itself is compatible with the specification of the .NET Framework 2.0, the included solution files target a later build for compatibility.

[dotnet-core]: https://dotnet.microsoft.com/download

## Building
First clone the repository (you will need git installed)

    git clone http://github.com/kyordhel/GPSRCmdGen.git

The building procedure ***should not*** depend on operating system.

    dotnet run --project GPSRCmdGen/GPSRCmdGen.csproj
    dotnet run --projcet EGPSRCmdGen/EGPSRCmdGen.csproj

Or if the `.csproj` file is in your current directory, you may simply `dotnet run`.

## Reuse in other competitions and projects
Thanks to the MIT license, you can adapt this project to your own needs (acknowledgments are always appreciated). Feel free to use this generator.

The generators use free context grammars that contain wild-cards which are replaced by random values from xml configuration files. The grammar's format specification can be found [here](https://github.com/kyordhel/GPSRCmdGen/wiki/Grammar-Format-Specification) and [here](https://github.com/kyordhel/GPSRCmdGen/blob/master/CommonFiles/FormatSpecification.txt)


## Contributing
Contributions and questions are always welcome
