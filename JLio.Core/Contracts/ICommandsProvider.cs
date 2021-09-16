namespace JLio.Core.Contracts
{
    public interface ICommandsProvider
    {
        ICommand this[string command] { get; }
    }
}