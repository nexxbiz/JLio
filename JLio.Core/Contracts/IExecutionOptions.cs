namespace JLio.Core.Contracts
{
    public interface IExecutionOptions
    {
        IItemsFetcher ItemsFetcher { get; set; }
        IExecutionLogger Logger { get; set; }
    }
}