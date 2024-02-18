namespace DotNetWhere.Core.Resolvers;

public interface IResolver<out TService>
{
    TService Get();
}
