# DotNetWhere architecture

The approved design separates invocation data from service dependencies. Search requests and output settings
are method arguments, never DI registrations. A single analysis service must handle successive requests with
different filters without rebuilding its dependencies.

Core owns an independent dependency graph, search filters and the pure graph traversal. Application owns the
analysis request/result and `IDependencyGraphSource`. Infrastructure implements this port using dotnet,
MSBuild and NuGet assets. The executable owns CLI parsing, formatters, streams and the composition root.
Application references Core; Infrastructure references Application/Core; the executable composes them.

Graph loading maps resolved versions from each project/target's assets into the independent graph. Search
results are distinct from the graph. Expected loading, invalid-filter and no-match failures are distinct
structured errors. Cancellation propagates. Unexpected defects are handled only at the CLI boundary.

Preserve current CLI flags, Color/Compact/Json formatting and the JSON envelope and property names. YAML
remains unsupported and should report an explicit error. The CLI owns files it opens and never closes caller
streams. Every formatter honors its supplied output. Keep net10.0 and centralized dependency versions.

Acceptance: repeated/concurrent independent requests, exact/wildcard/regex/negation, per-project/target graph
isolation, resolved-version ranges, output destinations, process errors/cancellation, cleanup, correct exit
codes, DI validation, architecture references and tool packaging.
