using DotNetWhere.Core.Graph;

namespace DotNetWhere.Application;

public interface IDependencyGraphSource
{
    Task<DependencyGraph> LoadAsync(string directory, CancellationToken cancellationToken);
}
