namespace JLio.Core.Contracts
{
    public interface IJLioExecutionOptions
    {
        IItemsFetcher ItemsFetcher { get; set; }
        IJLioExecutionLogger Logger { get; set; }
    }
}