namespace JLio.Core.Contracts;

public interface IFunctionsProviderRegistrar
{
    IFunctionsProviderRegistrar Register<T>() where T : IFunction;
}