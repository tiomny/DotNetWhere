# DotNetWhere Architecture Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Remove invocation state from DI and isolate graph analysis from CLI, files, processes and NuGet.

**Architecture:** Core owns the graph and search. Application orchestrates a graph-source port. Infrastructure
loads and normalizes NuGet data. The executable composes services and owns presentation/resources.

**Tech Stack:** C# 14, net10.0, NuGet.ProjectModel, CommandLineParser, Spectre.Console, Newtonsoft.Json, xUnit.

**Spec:** `docs/superpowers/specs/2026-10-09-architecture-design.md`

## Global Constraints

- Keep net10.0, current CLI flags and the existing JSON shape.
- No requests, search filters, CLI options or output files in DI registrations.
- Inner projects have no CLI, DI, NuGet or rendering dependencies.
- Expected errors carry a code and message; cancellation propagates; streams have one owner.

## Review Focus

- Empty/malformed negation and regex fail before loading a graph.
- Shared package names across projects/targets do not share search state.
- Requested version ranges resolve against actual lock-file versions.
- File destinations containing spaces and markup in error messages work.
- Cancellation and process failures clean temporary files and child processes.

## Task 1: Pure graph and request-scoped analysis

**Files:** Core graph/search/models; Application analysis contracts; `tests/DotNetWhere.Tests/AnalysisTests.cs`.

**Interfaces:** `AnalyzeDependencies.ExecuteAsync(AnalysisRequest, CancellationToken)` returns `AnalysisResult`;
`IDependencyGraphSource.LoadAsync(string, CancellationToken)` returns `DependencyGraph`.

- [x] Write tests for successive/different filters, independent projects/targets, ancestors, exact/wildcard/
  regex/negation, invalid filters, no-match and cancellation; observe missing-contract failures.
- [x] Implement independent graph models, per-call filter compilation/search and the application port/scenario.
- [x] Run the analysis tests and verify they pass without external files/processes.

## Task 2: Infrastructure graph adapter

**Files:** `src/DotNetWhere.Infrastructure/`; `tests/DotNetWhere.Tests/InfrastructureTests.cs`.

**Interfaces:** `NuGetDependencyGraphSource` implements Task 1's port; internal process runner accepts argument
lists and cancellation; `AssetsGraphReader` normalizes restore/lock files to Core graph models.

- [x] Write tests using assets fixtures for resolved ranges and project/target isolation, and controlled process
  fixtures for errors, spaces, cancellation and cleanup; observe failures before implementation.
- [x] Implement explicit sequential restore/graph generation, cancellable process execution and scoped temporary
  resources; translate expected IO/process/data failures into graph-source errors.
- [x] Run infrastructure and analysis tests.

## Task 3: CLI and presentation

**Files:** executable CLI/formatters/composition root; `tests/DotNetWhere.Tests/CliTests.cs`; project references.

**Interfaces:** `Cli.RunAsync(args, output, error, CancellationToken)` returns an exit code; explicit formatter
factory selects by format, and rendering accepts caller-owned streams.

- [x] Write tests for all supported formats/destinations, JSON compatibility, markup escaping, failure codes,
  unsupported YAML, invalid regex before output truncation and caller stream ownership; observe failures.
- [x] Move CLI/presentation out of Application, remove resolver/pipeline code and wire stable services without
  request data; enable DI validation; return the CLI code from Program.
- [x] Run the whole suite, CLI smoke checks, Release build and tool pack/install checks.

## Task 4: Documentation and final verification

- [x] Update README and repository guidance for the architecture and behavioral error contract.
- [x] Verify architecture references, formatting, tests, build and packaging; review the complete diff.

## Execution decisions

- User approved the presented design and requested implementation; no additional approval handoff is needed.
- Work in the clean shared checkout, leave changes uncommitted for review, and execute inline.
- Use separate immutable graph/result models, explicit factories and transient application/infrastructure
  registrations. Do not add a mediator, framework host or interfaces for pure algorithms.

## Execution record

- Task 1: complete. New-contract tests failed before implementation; 23 analysis tests then passed.
- Task 2: complete. Assets/process tests passed; malformed NuGet JSON was verified to raise FileFormatException
  and is translated at the infrastructure boundary.
- Task 3: complete. CLI tests passed; inner-layer assembly references and DI validation are covered.
- Task 4: complete. README/AGENTS updated; full Release suite passed 56/56; build, pack, package contents,
  tool install, installed `dotnet-where` and `dotnet where` commands verified.
- Ruling: user requested global nullable/analyzers and physical tests relocation. Move shared props to the
  repository root; place tests in `tests/DotNetWhere.Tests`; group solution items in `src`, `tests`, `common`.
- Ruling: use `--artifacts-path /tmp/dotnetwhere-architecture-artifacts` for verification because the shared
  IDE overwrites ordinary obj files with Windows package-cache paths. No product build workaround was added.
- Independent review found unbounded pipe draining after process exit. Both timeout/cancellation regressions
  failed before fixing linked cancellation for the entire process/read operation, then passed. Review of the
  fix found no further production defects. Readiness markers removed fixed-delay cancellation assumptions.
- Additional infrastructure coverage: multiple projects/targets sharing package names, generation failure/
  cancellation cleanup and descendant termination. An already exited parent's orphan cannot be discovered
  by the standard cross-platform Process API; pipe cancellation still bounds completion in that case.
- Verification: `dotnet test -c Release --artifacts-path /tmp/dotnetwhere-architecture-artifacts --logger trx
  --results-directory ./tests-results` -> 56 passed, 0 failed, 0 skipped.
- Verification: tool pack/install and 33 CLI/package checks -> passed, using isolated fixtures/cache in /tmp.
- Changes remain uncommitted in the shared checkout for review.
