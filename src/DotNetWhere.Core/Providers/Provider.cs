using DotNetWhere.Core.Matchers;
using DotNetWhere.Core.Models;

namespace DotNetWhere.Core.Providers;

internal sealed class Provider(
    IPackageMatcher? packageMatcher,
    ITargetMatcher? targetMatcher
    ) : IProvider
{
    public Response Get(Request request)
    {
        var name = Path.GetFileName(request.WorkingDirectory);

        try
        {
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
                            targetMatcher,
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