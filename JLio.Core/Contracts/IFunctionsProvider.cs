namespace JLio.Core.Contracts;

public interface IFunctionsProvider
{
    IFunction this[string functionName] { get; }
}