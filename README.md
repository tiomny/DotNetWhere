| ![](https://raw.githubusercontent.com/tiomny/DotNetWhere/master/assets/logo/256/logo.png) |
|:--:|
| `dotnet where` - a .NET global tool to show information about where a NuGet package is installed |
| ![Nuget](https://img.shields.io/nuget/v/DotNetWhere?label=version) ![GitHub](https://img.shields.io/github/license/tiomny/DotNetWhere) ![Nuget](https://img.shields.io/nuget/dt/DotNetWhere) ![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/tiomny/DotNetWhere/build.yml?branch=master) ![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/tiomny/DotNetWhere/release.yml?label=release) ![GitHub issues](https://img.shields.io/github/issues/tiomny/DotNetWhere) ![GitHub pull requests](https://img.shields.io/github/issues-pr/tiomny/DotNetWhere) |

## Installation

Download and install the [.NET 6/8 SDK](https://www.microsoft.com/net/download).

Once installed, run the following command:

```bash
> dotnet tool install -g DotNetWhere
```

## Example

```bash
> dotnet where -p Newtonsoft.Json
DotNetWhere.Core
  net6.0
    NuGet.ProjectModel: 6.8.0
      NuGet.DependencyResolver.Core: 6.8.0
        NuGet.Protocol: 6.8.0
          NuGet.Packaging: 6.8.0
            Newtonsoft.Json: 13.0.3
  net8.0
    NuGet.ProjectModel: 6.8.0
      NuGet.DependencyResolver.Core: 6.8.0
        NuGet.Protocol: 6.8.0
          NuGet.Packaging: 6.8.0
            Newtonsoft.Json: 13.0.3

```

```bash
> dotnet where -d E:\dev\DotNetWhere -p System.* -V /^(?!.*6\.0\.0).*
DotNetWhere.Application
  net6.0
    Spectre.Console.Cli: 0.48.0
      Spectre.Console: 0.48.0
        System.Memory: 4.5.5
DotNetWhere.Core
  net6.0
    NuGet.ProjectModel: 6.8.0
      NuGet.DependencyResolver.Core: 6.8.0
        NuGet.Configuration: 6.8.0
          System.Security.Cryptography.ProtectedData: 4.4.0
        NuGet.Protocol: 6.8.0
          NuGet.Packaging: 6.8.0
            NuGet.Configuration: 6.8.0
              System.Security.Cryptography.ProtectedData: 4.4.0
            System.Security.Cryptography.Pkcs: 6.0.4
```

## Usage

All query arguments are optional:
```bash
> dotnet where Newtonsoft.Json --version 13.0.1
> dotnet where Newtonsoft.Json -v 13.0.2
> dotnet where --help
  -d, --dir                Solution directory. When not passed: current
                           directory.

  -p, --package            The NuGet package name. When not passed: all
                           packages. Can be mask or regex.

  -V, --package-version    The NuGet package version. When not passed: any
                           version. Can be mask or regex.

  -f, --format             Output format. Default: Compact. Valid values:
                           Compact, Color, Yaml, Json

  -o, --output             Output file.

  --help                   Display this help screen.

  --version                Display version information.
```