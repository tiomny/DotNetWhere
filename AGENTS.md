# Repository Guidelines

## Project Structure & Module Organization

DotNetWhere is a .NET global tool that traces NuGet package dependencies. `DotNetWhere.sln` contains four production
projects and a test project:

- `src/DotNetWhere/`: CLI, formatters, composition root, entry point and global-tool packaging.
- `src/DotNetWhere.Application/`: analysis scenario, request/result contracts and dependency-graph source port.
- `src/DotNetWhere.Core/`: independent graph models, filters and pure dependency search.
- `src/DotNetWhere.Infrastructure/`: dotnet process execution, NuGet assets and temporary graph files.
- `tests/DotNetWhere.Tests/`: xUnit scenario, infrastructure, CLI and architecture tests.

Shared build settings live in `Directory.Build.props`; dependency versions are centralized in
`Directory.Packages.props`. GitHub workflows are in `.github/workflows/`. The solution groups production projects under `src`, tests under
`tests`, and shared configuration/documentation links under `common`.

Application references Core; Infrastructure references Application/Core; the executable composes these layers.
Core and Application must not reference NuGet, DI, CLI or rendering packages. Requests, search filters and output
destinations are invocation arguments, never DI registrations. The CLI owns files it opens; formatters borrow streams.

## Build, Test, and Development Commands

Run commands from the repository root. `global.json` requests SDK 10.0.401 with `latestPatch` roll-forward;
all projects target `net10.0` and use C# 14.

- `dotnet restore`: restore all solution dependencies.
- `dotnet build --no-restore`: build the solution; warnings are treated as errors.
- `dotnet test --no-restore --logger trx --results-directory ./tests-results`: match the CI test command.
- `dotnet run --project src/DotNetWhere -f net10.0 -- -d . -p Newtonsoft.Json --format Json`: inspect repository dependencies.
- `dotnet pack src/DotNetWhere/DotNetWhere.csproj -c Release -o ./artifacts`: create the distributable tool package.

## Coding Style & Naming Conventions

Follow `.editorconfig`: UTF-8, LF endings, four-space indentation, a 120-character line limit, and trimmed trailing whitespace. YAML uses two spaces; final newlines are not required.

Match existing C# patterns: file-scoped namespaces, PascalCase types and methods, camelCase parameters and locals, and `I`-prefixed interfaces. Prefer `var`, expression-bodied members, and modern C# syntax where appropriate. Nullable reference types and IDisposableAnalyzers are enabled globally in `Directory.Build.props`. Keep package versions centralized and address `IDisposableAnalyzers` diagnostics.

## Testing Guidelines

Tests use xUnit in `DotNetWhere.Tests`; no coverage threshold is configured. Exercise exact, wildcard, regex and negated
filters, repeat requests on one scenario, affected output formats and stream ownership. Infrastructure tests use assets
fixtures and a controlled child process rather than network restore. Verify `net10.0` builds and keep architecture tests
passing. Use descriptive names such as `Execute_DifferentRequests_UsesEachRequestsFilters`.

## Commit & Pull Request Guidelines

History uses short, plain-language subjects such as `Build fix` and `More search options`; no mandatory prefix convention is evident. Keep commits focused. For pull requests targeting `master`, describe the problem, behavior changes, related issues, and validation commands/results. Include sample CLI output when formatting changes, and update `README.md` when options or usage change. Ensure the build workflow passes.
