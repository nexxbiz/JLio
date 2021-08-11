namespace JLio.Core.Contracts
{
    public interface IJLioCommandsProviderRegistrar
    {
        IJLioCommandsProviderRegistrar Register<T>() where T : IJLioCommand;
    }
}