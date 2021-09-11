namespace JLio.Core.Contracts
{
    public interface ICommandsProviderRegistrar
    {
        ICommandsProviderRegistrar Register<T>() where T : ICommand;
    }
}