namespace JLio.Core.Contracts
{
    public interface IJLioFunctionsProvider
    {
        IJLioFunction this[string functionName] { get; }
    }
}