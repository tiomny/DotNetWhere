| ![](https://raw.githubusercontent.com/tiomny/DotNetWhere/master/assets/logo/256/logo.png) |
|:--:|
| `dotnet why` - a .NET global tool to show information about why a NuGet package is installed |
| ![Nuget](https://img.shields.io/nuget/v/DotNetWhere?label=version) ![GitHub](https://img.shields.io/github/license/tiomny/DotNetWhere) ![Nuget](https://img.shields.io/nuget/dt/DotNetWhere) ![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/tiomny/DotNetWhere/build.yml?branch=master) ![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/tiomny/DotNetWhere/release.yml?label=release) ![GitHub issues](https://img.shields.io/github/issues/tiomny/DotNetWhere) ![GitHub pull requests](https://img.shields.io/github/issues-pr/tiomny/DotNetWhere) |

## Installation

Download and install the [.NET 6/8 SDK](https://www.microsoft.com/net/download).

Once installed, run the following command:

```bash
> dotnet tool install -g DotNetWhere
```

## Example

```bash
> dotnet why Newtonsoft.Json

Why...?
* is the *Newtonsoft.Json* package
* in the C:\DotNetWhere directory

Answer:
*   DotNetWhere                  [2]
**  DotNetWhere.Core           [2/2]
*** net6.0                   [1/2]
1.  NuGet.ProjectModel (6.7.0) > NuGet.DependencyResolver.Core (6.7.0) > NuGet.Protocol (6.7.0) >
      NuGet.Packaging (6.7.0) > Newtonsoft.Json (13.0.1)
*** net8.0                   [1/2]
1.  NuGet.ProjectModel (6.7.0) > NuGet.DependencyResolver.Core (6.7.0) > NuGet.Protocol (6.7.0) >
      NuGet.Packaging (6.7.0) > Newtonsoft.Json (13.0.1)

Time elapsed: 00:00:03.01
```

## Usage

The mandatory query argument for `dotnet why` command is package name:

```bash
> dotnet why Newtonsoft.Json
```

At this moment, the only additional optional query argument for `dotnet why` command is `--version|-v` option:
```bash
> dotnet why Newtonsoft.Json --version 13.0.1
> dotnet why Newtonsoft.Json -v 13.0.2
```