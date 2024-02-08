using DotNetWhere.Core.Models;

namespace DotNetWhere.Application.Commands;

internal static class Extensions
{
    public static Request ToRequest(this Options options) =>
        new(options.PackageName, options.Directory)
        {
            PackageVersion = options.PackageVersion
        };
}