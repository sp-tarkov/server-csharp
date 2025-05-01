# Single Player Tarkov - Server Project

This is the Server project for the Single Player Tarkov mod for Escape From Tarkov. It can be run locally to replicate responses to the modified Escape From Tarkov client


# Table of Contents

- [Features](#features)
- [Installation](#installation)
  - [Requirements](#requirements)
  - [Initial Setup](#initial-setup)
- [Development](#development)
  - [Commands](#commands)
  - [Debugging](#debugging)
  - [Mod Debugging](#mod-debugging)
- [Deployment](#deployment)
- [Contributing](#contributing)
  - [Branches](#branchs)
  - [Pull Request Guidelines](#pull-request-guidelines)
  - [Tests](#tests)
- [License](#license)

## Features

For a full list of features, please see [FEATURES.md](FEATURES.md)

## Installation

### Requirements

This project has been built in [Visual Studio](https://visualstudio.microsoft.com/) (VS) and [Rider](https://www.jetbrains.com/rider/) using [.NET](https://dotnet.microsoft.com/en-us/)

Minimum required Visual Studio version is `17.13.5`
Minimum required Rider version is `2024.3`

You only need one of the above.

### Initial Setup

1. Download and install the [.net 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).
2. Run `git clone https://github.com/sp-tarkov/server-csharp.git server` to clone the repository
3. Run `git lfs pull` to download LFS files locally.
4. Open the `project/server-csharp.sln` file in Visual Studio or Rider
5. Run `Build > Build Solution (CTRL + SHIFT + B)` in the IDE

## Development

### Commands

### Debugging

To debug the project in Visual Studio Code:[SPTarkov.Server.Core.csproj](Libraries/SPTarkov.Server.Core/SPTarkov.Server.Core.csproj)
1. Choose `Server` and `Spt Server` in the debug dropdowns
2. Choose `Debug > Start Debugging (F5)` to run the server
[SPTarkov.Server.Core.csproj](Libraries/SPTarkov.Server.Core/SPTarkov.Server.Core.csproj)
With Rider:
1. Choose the configuration called `SPTarkov.Server: Spt Server Debug`
2. Hit `(Alt + F5)` To start Debugging

### Mod Debugging

To debug a server mod in Visual Studio, you can copy the mod DLL into the `user/mods` folder and then start the server

## Deployment

To build the project via CLI:
1. Open the terminal at the poject root
2. Run command `dotnetn publish`
- `-c Release` for release build
- `-p:SptVersion=*.*.*` to set the version ProgramStatics uses
- `-p:SptCommit=******` to set the commit ProgramStatics uses
- `-p:SptBuildTime=*********` to set the buildTime ProgramStatic uses
- `-p:SptBuildType=*********` to set the BuildType ProgramStatic uses
- - Options for SptBuildType are in the EntryType Enum
- - LOCAL, DEBUG, RELEASE, BLEEDING_EDGE, BLEEDING_EDGE_MODS - *must be all caps*

## Contributing

We're really excited that you're interested in contributing! Before submitting your contribution, please consider the following:

### Branches

- **master**
  The default branch used for the latest stable release. This branch is protected and typically is only merged with release branches.
- **development**
  The main branch for server development. PRs should target this.

### Pull Request Guidelines

- **Keep Them Small**
  If you're fixing a bug, try to keep the changes to the bug fix only. If you're adding a feature, try to keep the changes to the feature only. This will make it easier to review and merge your changes.
- **Perform a Self-Review**
  Before submitting your changes, review your own code. This will help you catch any mistakes you may have made.
- **Remove Noise**
  Remove any unnecessary changes to white space, code style formatting, or some text change that has no impact related to the intention of the PR.
- **Create a Meaningful Title**
  When creating a PR, make sure the title is meaningful and describes the changes you've made.
- **Write Detailed Commit Messages**
  Bring out your table manners, speak the Queen's English and be on your best behaviour.

### Style Guide

 TODO: style guidance
 Ensure that your code is automatically formatted whenever you save a file.

### Tests

We have a number of tests that are run automatically when you submit a pull request. You can run these tests locally by running The unit test sub-project. If you're adding a new feature or fixing a bug, please conceder adding tests to cover your changes so that we can ensure they don't break in the future.

## License

This project is licensed under the NCSA Open Source License. See the [LICENSE](LICENSE.md) file for details.
