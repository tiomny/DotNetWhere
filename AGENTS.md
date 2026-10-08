# Repository Guidelines

## Project Structure & Module Organization

DotNetWhere is a .NET global tool that traces NuGet package dependencies. `DotNetWhere.sln` contains three projects:

- `src/DotNetWhere/`: executable entry point and global-tool packaging.
- `src/DotNetWhere.Application/`: CLI options, dependency injection, commands, and Color/Compact/Json/Yaml output writers.
- `src/DotNetWhere.Core/`: dependency queries, models, matchers, validators, and .NET process execution.

Shared build settings live in `src/Directory.Build.props`; dependency versions are centralized in `src/Directory.Packages.props`. GitHub workflows are in `.github/workflows/`. There are currently no test projects or dedicated asset directories.

## Build, Test, and Development Commands

Run commands from the repository root. `global.json` requests SDK 8.0.100 with `latestMajor` roll-forward; projects target both `net6.0` and `net8.0`.

- `dotnet restore`: restore all solution dependencies.
- `dotnet build --no-restore`: build both target frameworks; warnings are treated as errors.
- `dotnet test --no-restore --logger trx --results-directory ./tests-results`: match the CI test command.
- `dotnet run --project src/DotNetWhere -f net8.0 -- -d . -p Newtonsoft.Json --format Json`: inspect dependencies in this repository using the local executable.
- `dotnet pack src/DotNetWhere/DotNetWhere.csproj -c Release -o ./artifacts`: create the distributable tool package.

## Coding Style & Naming Conventions

Follow `.editorconfig`: UTF-8, LF endings, four-space indentation, a 120-character line limit, and trimmed trailing whitespace. YAML uses two spaces; final newlines are not required.

Match existing C# patterns: file-scoped namespaces, PascalCase types and methods, camelCase parameters and locals, and `I`-prefixed interfaces. Prefer `var`, expression-bodied members, and modern C# syntax where appropriate. Nullable reference types are enabled in Core and Application. Keep package versions centralized and address `IDisposableAnalyzers` diagnostics.

## Testing Guidelines

No testing framework or coverage threshold is currently configured. The CI test command does not establish behavioral coverage without test projects. For changes, exercise exact, wildcard, regex, and negated filters, plus affected output formats. Verify both target frameworks build. If adding automated tests, use a separate `*.Tests` project and descriptive names such as `Match_ExactName_ReturnsTrue`.

## Commit & Pull Request Guidelines

History uses short, plain-language subjects such as `Build fix` and `More search options`; no mandatory prefix convention is evident. Keep commits focused. For pull requests targeting `master`, describe the problem, behavior changes, related issues, and validation commands/results. Include sample CLI output when formatting changes, and update `README.md` when options or usage change. Ensure the build workflow passes.
