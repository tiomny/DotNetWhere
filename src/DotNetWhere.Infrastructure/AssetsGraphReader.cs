using DotNetWhere.Application;
using DotNetWhere.Core.Graph;

using Newtonsoft.Json;

using NuGet.Common;
using NuGet.LibraryModel;
using NuGet.ProjectModel;

namespace DotNetWhere.Infrastructure;

public sealed class AssetsGraphReader
{
    public DependencyGraph Read(string graphPath, string name)
    {
        try
        {
            var spec = DependencyGraphSpec.Load(graphPath);
            var projects = spec.Projects
                .Where(project => project.RestoreMetadata.ProjectStyle == ProjectStyle.PackageReference)
                .Select(ReadProject)
                .ToArray();
            return new DependencyGraph(name, projects);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
            or JsonException or System.Text.Json.JsonException or FileFormatException or ArgumentException
            or FormatException)
        {
            throw new GraphSourceException(AnalysisErrorCode.InvalidAssets,
                $"Could not read dependency assets: {exception.Message}", exception);
        }
    }

    private static ProjectGraph ReadProject(PackageSpec project)
    {
        var path = Path.Combine(project.RestoreMetadata.OutputPath, "project.assets.json");
        var lockFile = LockFileUtilities.GetLockFile(path, NullLogger.Instance)
            ?? throw InvalidAssets($"Missing or invalid assets file: '{path}'.");

        var targets = project.TargetFrameworks.Select(framework =>
        {
            var target = lockFile.Targets.FirstOrDefault(candidate =>
                candidate.TargetFramework.Equals(framework.FrameworkName) &&
                string.IsNullOrEmpty(candidate.RuntimeIdentifier))
                ?? throw InvalidAssets($"Target '{framework.FrameworkName}' is missing in '{path}'.");

            var libraries = target.Libraries.ToDictionary(
                library => library.Name ?? throw InvalidAssets($"An unnamed library was found in '{path}'."),
                StringComparer.OrdinalIgnoreCase);
            var packages = target.Libraries
                .Where(library => library.Type == "package")
                .Select(library => new ResolvedPackage(
                    library.Name ?? throw InvalidAssets($"An unnamed package was found in '{path}'."),
                    (library.Version ?? throw InvalidAssets($"A package version is missing in '{path}'."))
                        .ToNormalizedString(),
                    library.Dependencies.Where(dependency =>
                    {
                        if (!libraries.TryGetValue(dependency.Id, out var resolved))
                            throw InvalidAssets($"Dependency '{dependency.Id}' is missing in '{path}'.");
                        return resolved.Type == "package";
                    }).Select(dependency => dependency.Id)))
                .ToArray();

            var roots = framework.Dependencies
                .Where(dependency => (dependency.LibraryRange.TypeConstraint & LibraryDependencyTarget.Package) != 0)
                .Select(dependency =>
                {
                    if (!libraries.TryGetValue(dependency.Name, out var resolved) || resolved.Type != "package")
                        throw InvalidAssets($"Package '{dependency.Name}' is missing in '{path}'.");
                    return resolved.Name ?? throw InvalidAssets($"An unnamed package was found in '{path}'.");
                });

            return new TargetGraph(framework.FrameworkName.GetShortFolderName(), roots, packages);
        });

        return new ProjectGraph(project.Name, project.Version.ToNormalizedString(), targets);
    }

    private static GraphSourceException InvalidAssets(string message) =>
        new(AnalysisErrorCode.InvalidAssets, message);
}
