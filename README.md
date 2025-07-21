# Single Player Tarkov - Server Project

This is the Server project for the Single Player Tarkov mod for Escape From Tarkov. It can be run locally to replicate responses to the modified Escape From Tarkov client.


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
  - [Git LFS](#git-large-file-storage-lfs)
  - [Style Guide](#style-guide)
  - [Tests](#tests)
- [License](#license)

## Features

For a full list of features, please see [FEATURES.md](FEATURES.md)

## Installation

### Requirements

This project has been built in [Visual Studio](https://visualstudio.microsoft.com/) (VS) and [Rider](https://www.jetbrains.com/rider/) using [.NET](https://dotnet.microsoft.com/en-us/)

One of the following is required:
- Minimum Visual Studio version required: `17.13.5`
- Minimum Rider version required: `2024.3`

### Initial Setup

1. Download and install the [.net 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).
2. Download and install [Powershell 7.5](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.5) as this is required for post build scripts
3. Run `git clone https://github.com/sp-tarkov/server-csharp.git server` to clone the repository
4. Run `git lfs pull` to download LFS files locally.
5. Open the `project/server-csharp.sln` file in Visual Studio or Rider
6. Run `Build > Build Solution (CTRL + SHIFT + B)` in the IDE

## Development

### Commands

### Debugging

To debug the project in Visual Studio:
1. Choose `Server` and `Spt Server` in the debug drop-downs
2. Choose `Debug > Start Debugging (F5)` to run the server

And in Rider:
1. Choose the configuration called `SPTarkov.Server: Spt Server Debug`
2. Press `(Alt + F5)` to start debugging

### Mod Debugging

To debug a server mod in Visual Studio, you can copy the mod DLL into the `user/mods` folder and then start the server.

## Deployment

To build the project via CLI:
1. Open the terminal at the project root
2. Run the command `dotnet publish`
    - `-c Release` for release build
    - `-p:SptVersion=*.*.*` to set the version ProgramStatics uses
    - `-p:SptCommit=******` to set the commit ProgramStatics uses
    - `-p:SptBuildTime=*********` to set the buildTime ProgramStatic uses
    - `-p:SptBuildType=*********` to set the BuildType ProgramStatic uses
    - Options for `SptBuildType`: `LOCAL`, `DEBUG`, `RELEASE`, `BLEEDING_EDGE`, `BLEEDING_EDGE_MODS`

## Contributing

We're really excited that you're interested in contributing! Before submitting your contribution, please consider the following:

### Branches

- **master**
  The default branch used for the latest stable release. This branch is protected and typically is only merged with release branches.
- **development**
  The main branch for server development. **PRs should target this.**

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

### Git Large File Storage (LFS)

We use a custom git LFS server to store large files. The public server is read-only. If you are adding or modifying large files, you will need to create an issue with details about this change so that a project developer can make them to a writable endpoint of the LFS server. Bonus points if you include a patch file in the issue. **You will not be able to submit a pull request for LFS file changes.**

### Style Guide

We use [CSharpier](https://csharpier.com/) to keep the project's code styled/formatted. You can install it globally by running: `dotnet tool install -g csharpier`. You may then apply the formatting rules by running: `csharpier format .`. Please ensure this is ran before your PR is created to make merges easier. A workflow will fail if formatting changes are required.

#### Format On Save

There are Plugins for both [VS](https://marketplace.visualstudio.com/items?itemName=csharpier.csharpier-vscode) and [Rider](https://plugins.jetbrains.com/plugin/18243-csharpier) which allow you to automatically format project code when a file is saved.

In Rider, after installing the CSharpier plugin:
- Open `Settings`
- Browse to `Editor`, `Code Style`:
    - Set scheme to `Project`
    - Check `Enable EditorConfig Support`
- Browse to `Tools`, `CSharpier`:
    - Enable "Run On Save"
- Browse to `Tools`, `Actions on save`:
    - Check `Reformat and Cleanup Code`
    - Set to `Reformat & Apply Syntax Style`, `Changed lines`

### Tests

We have a number of tests that are run automatically when you submit a pull request. You can run these tests locally by running The unit test sub-project. If you're adding a new feature or fixing a bug, please consider adding tests to cover your changes so that we can ensure they don't break in the future.

## License

This project is licensed under the NCSA Open Source License. See the [LICENSE](LICENSE.md) file for details.
