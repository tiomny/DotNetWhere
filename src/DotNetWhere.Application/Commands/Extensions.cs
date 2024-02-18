using DotNetWhere.Core.Models;

namespace DotNetWhere.Application.Commands;

internal static class Extensions
{
    public static Request ToRequest(this Options options) =>
        new(options.Directory ?? Environment.CurrentDirectory)
        {
            PackageName = options.PackageName,
            PackageVersion = options.PackageVersion,
            Target = options.Target,
        };
}