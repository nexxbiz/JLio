namespace JLio.Core.Contracts
{
    public interface IJLioFunctionsProvider
    {
        IFunction this[string functionName] { get; }
    }
}