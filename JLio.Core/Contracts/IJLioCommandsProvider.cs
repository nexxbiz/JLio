namespace JLio.Core.Contracts
{
    public interface IJLioCommandsProvider
    {
        IJLioCommand this[string command] { get; }
    }
}