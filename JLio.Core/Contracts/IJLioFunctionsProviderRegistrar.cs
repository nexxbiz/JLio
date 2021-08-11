namespace JLio.Core.Contracts
{
    public interface IJLioFunctionsProviderRegistrar
    {
        IJLioFunctionsProviderRegistrar Register<T>() where T : IJLioFunction;
    }
}