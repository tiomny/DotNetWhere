using DotNetWhere.Core.Models;

namespace DotNetWhere.Application.Commands;

internal static class Extensions
{
    public static Request ToRequest(this DotNetWhereCommand.Settings settings) =>
        new(settings.PackageName, settings.Directory)
        {
            PackageVersion = settings.PackageVersion
        };
}