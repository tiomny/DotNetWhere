using DotNetWhere.Core.Models;

namespace DotNetWhere.Core.Validations;

internal sealed class RequestValidator
(
#pragma warning disable CS9113 // Parameter is unread.
    Request request
#pragma warning restore CS9113 // Parameter is unread.
) : IResultHandler
{

#pragma warning disable S1135 // Track uses of "TODO" tags
                             //TODO: add some validations
    public Result Handle() =>        
            Result.Success();
#pragma warning restore S1135 // Track uses of "TODO" tags

    private static class Errors
    {
        public const string PackageNameNotSpecified =
            "Package name not specified. Please run command once again specifying package name - 'dotnet why PACKAGE_NAME'.";
    }
}