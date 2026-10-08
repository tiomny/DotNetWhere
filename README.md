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
DotNetWhere.Infrastructure
  net10.0
    NuGet.ProjectModel: 7.9.0
      NuGet.DependencyResolver.Core: 7.9.0
        NuGet.Protocol: 7.9.0
          NuGet.Packaging: 7.9.0
            Newtonsoft.Json: 13.0.3
DotNetWhere.Tests
  net10.0
    Microsoft.NET.Test.Sdk: 17.14.1
      Microsoft.TestPlatform.TestHost: 17.14.1
        Newtonsoft.Json: 13.0.4
DotNetWhere
  net10.0
    Newtonsoft.Json: 13.0.4
Time elapsed: 00:00:01.54
```

```bash
> dotnet where -d . -p 'System.*' -V '!/.*6\.0\.0.*' -t net10.0 --format Compact
```

The first example shows the paths to Newtonsoft.Json in this solution. The second command combines a wildcard name,
a negated regex version and a target-framework filter.

## Development

The repository uses stable SDK `10.0.401` from `global.json`, with patch roll-forward enabled.
All projects target `net10.0` and use C# 14. Nullable reference types and IDisposableAnalyzers are enabled globally
in `Directory.Build.props`. Build and run from the repository root:

```bash
dotnet restore
dotnet build --no-restore
dotnet run --project src/DotNetWhere -f net10.0 -- -d . -p Newtonsoft.Json --format Json
dotnet pack src/DotNetWhere/DotNetWhere.csproj -c Release -o ./artifacts
```

`dotnet test --no-restore --logger trx --results-directory ./tests-results` runs the CI test command.
The xUnit suite covers successive and concurrent requests, filtering, assets parsing, processes, output streams and
architecture boundaries. Process tests use a controlled executable fixture; they do not restore packages from a network.

## Architecture

- `DotNetWhere.Core`: immutable dependency graphs, search filters and a pure graph traversal.
- `DotNetWhere.Application`: `AnalyzeDependencies`, operation requests/results and `IDependencyGraphSource`.
- `DotNetWhere.Infrastructure`: dotnet processes, NuGet assets normalization and temporary-file ownership.
- `DotNetWhere`: CLI parsing, output formatters, output files and dependency registration.

Application depends on Core. Infrastructure implements the Application port and converts NuGet data to Core models.
The executable composes the services. Tests live in `tests/DotNetWhere.Tests`; the solution groups production projects
under `src`, tests under `tests`, and shared files under `common`. Core and Application have no DI, CLI, NuGet or rendering dependencies.

Search parameters are passed to `ExecuteAsync` on every invocation. The same scenario can handle different requests
without rebuilding a container. Output settings stay in the CLI. Formatters borrow streams, and the CLI closes only
files it opens. Project/target graphs use resolved package versions; search results are separate from the input graph.

## Usage

All query arguments are optional. Run `dotnet where --help` for examples and the full option list.

| Option | Meaning |
| --- | --- |
| `-d`, `--dir` | Project or solution directory; defaults to the current directory |
| `-p`, `--package` | Package name filter |
| `-V`, `--package-version` | Resolved package version filter |
| `-t`, `--dotnet-target` | Target framework filter |
| `-f`, `--format` | Color (default), Compact or Json; Yaml is reserved but unsupported |
| `-o`, `--output` | Write a successful result to this file |
| `--help`, `--version` | Display help or version without running analysis |

Filters support exact strings, `*`/`?` wildcards, a leading `/` for regex and a leading `!` for negation. Name and version
filters are combined with AND. Regex matching is case-sensitive and limited to one second per match; exact strings
and wildcards are case-insensitive. Matching packages and their ancestor paths are retained in the output.

Json retains the `{"solution": ..., "elapsedTime": ...}` success envelope and `{"errors": [...]}` error envelope.
Color, Compact and Json all honor `--output`. A failed query leaves the destination file untouched and renders its
errors to standard output in the selected format. Parsing errors, unsupported formats, output failures and unexpected
exceptions are reported to standard error.

Exit codes: `0` for success/help/version, `1` for analysis or output failure (including no matches), `2` for invalid
arguments or unsupported Yaml, and `130` for cancellation. Ctrl+C cancels the operation and stops its dotnet process.
