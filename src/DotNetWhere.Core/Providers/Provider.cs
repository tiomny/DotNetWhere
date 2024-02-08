using DotNetWhere.Core.Matchers;
using DotNetWhere.Core.Models;

namespace DotNetWhere.Core.Providers;

internal sealed class Provider : IProvider
{
    public Response Get(Request request)
    {
        var name = Path.GetFileName(request.WorkingDirectory);

        try
        {
            var packageMatcher = MatcherFactory.CreateMatcher(request.PackageName);
            var versionMatcher = MatcherFactory.CreateMatcher(request.PackageVersion);

            return new RequestValidator(request)
                .HandleWith(new WorkingDirectoryValidator(request.WorkingDirectory))
                .HandleNext(new GetRestoreGraphOutputPathQuery())
                .Map(restoreGraphOutputPath =>
                    new GenerateRestoreGraphFileCommand(
                            request.WorkingDirectory,
                            restoreGraphOutputPath)
                        .HandleWith(new RestoreCommand(request.WorkingDirectory))
                        .HandleNext(new GetProjectPackagesQuery(
                            packageMatcher,
                            versionMatcher,
                            restoreGraphOutputPath,
                            name)))
                .Unwrap()
                .ToResponse();
        }
        catch (Exception exception)
        {
            return Result<Solution?>
                .Failure(exception.Message)
                .ToResponse();
        }
    }
}