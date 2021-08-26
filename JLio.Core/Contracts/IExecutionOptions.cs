namespace JLio.Core.Contracts
{
    public interface IExecutionOptions
    {
        IItemsFetcher ItemsFetcher { get; set; }
        IJLioExecutionLogger Logger { get; set; }
    }
}