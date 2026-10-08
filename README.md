||
|:--:|
| `dotnet where` - a .NET global tool to show information about where a NuGet package is installed. |
| Fork of [dotnet why](https://github.com/tadamczyk/DotNetWhy) but with advanced features. |
| ![Nuget](https://img.shields.io/nuget/v/DotNetWhere?label=version) ![GitHub](https://img.shields.io/github/license/tiomny/DotNetWhere) ![Nuget](https://img.shields.io/nuget/dt/DotNetWhere) ![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/tiomny/DotNetWhere/build.yml?branch=master) ![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/tiomny/DotNetWhere/release.yml?label=release) ![GitHub issues](https://img.shields.io/github/issues/tiomny/DotNetWhere) ![GitHub pull requests](https://img.shields.io/github/issues-pr/tiomny/DotNetWhere) |

## Installation

Download and install the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).
DotNetWhere targets `net10.0` and requires .NET 10 to run.

Once installed, run the following command:

```bash
> dotnet tool install -g DotNetWhere
```

## Example

```bash
> dotnet where -p Newtonsoft.Json --format Compact
DotNetWhere.Application
  net10.0
    Newtonsoft.Json: 13.0.4
DotNetWhere.Core
  net10.0
    NuGet.ProjectModel: 7.9.0
      NuGet.DependencyResolver.Core: 7.9.0
        NuGet.Protocol: 7.9.0
          NuGet.Packaging: 7.9.0
            Newtonsoft.Json: 13.0.3
Time elapsed: 00:00:01.54
```

```bash
> dotnet where -d . -p 'System.*' -V '!/.*6\.0\.0.*' -t net10.0 --format Compact
DotNetWhere.Application
  net10.0
    Microsoft.SourceLink.GitHub: 10.0.401
      Microsoft.Build.Tasks.Git: 10.0.401
        System.IO.Hashing: 10.0.12
      System.IO.Hashing: 10.0.12
DotNetWhere.Core
  net10.0
    Microsoft.SourceLink.GitHub: 10.0.401
      Microsoft.Build.Tasks.Git: 10.0.401
        System.IO.Hashing: 10.0.12
      System.IO.Hashing: 10.0.12
    NuGet.ProjectModel: 7.9.0
      NuGet.DependencyResolver.Core: 7.9.0
        NuGet.Configuration: 7.9.0
          System.Security.Cryptography.ProtectedData: 8.0.0
        NuGet.Protocol: 7.9.0
          NuGet.Packaging: 7.9.0
            NuGet.Configuration: 7.9.0
              System.Security.Cryptography.ProtectedData: 8.0.0
            System.Security.Cryptography.Pkcs: 8.0.1
DotNetWhere
  net10.0
    Microsoft.SourceLink.GitHub: 10.0.401
      Microsoft.Build.Tasks.Git: 10.0.401
        System.IO.Hashing: 10.0.12
      System.IO.Hashing: 10.0.12
Time elapsed: 00:00:01.58
```

These examples were captured in this repository after restoring its dependencies.

## Development

The repository uses stable SDK `10.0.401` from `global.json`, with patch roll-forward enabled.
All three projects target `net10.0` and use C# 14. Build and run from the repository root:

```bash
dotnet restore
dotnet build --no-restore
dotnet run --project src/DotNetWhere -f net10.0 -- -d . -p Newtonsoft.Json --format Json
dotnet pack src/DotNetWhere/DotNetWhere.csproj -c Release -o ./artifacts
```

`dotnet test --no-restore --logger trx --results-directory ./tests-results` runs the CI test command.
There are currently no test projects in the solution.

## Usage

All query arguments are optional:
```
> dotnet where --help
USAGE:
List all packages in solution in current directory with compact format:
  DotNetWhere --format Compact
List packages in target net10.0 that name matches System.* and version is not
6.0.0:
  DotNetWhere -p System.* -t net10.0 -V !6.0.0

  -d, --dir                Solution directory. When not passed: current
                           directory.

  -p, --package            The NuGet package name. ***

  -V, --package-version    The NuGet package version. ***

  -t, --dotnet-target      .Net target version. ***

  -f, --format             Output format. Default: Color. Valid values: Color,
                           Compact, Yaml, Json

  -o, --output             Output file.

  --help                   Display this help screen.

  --version                Display version information.


*     When not passed: any.
**    Can be mask or regex.
***   search expression can start from `!` or `/`.
        `!` is for `not`.
            Everything afer it will be considered as a `not match` expression.
            The app looks for everything that does not match expression after
            `!`.
        `/` is for `regular expression`.
            The app looks for everything that match regular expression after
            `/`.
```
