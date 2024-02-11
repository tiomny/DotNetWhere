namespace DotNetWhere.Application.Factories;

internal interface IFactory<TService>
{
    TService Create();
}
