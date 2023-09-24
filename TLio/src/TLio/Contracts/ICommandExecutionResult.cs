namespace TLio.Contracts
{
    public interface ICommandExecutionResult<T>
    {
        public IReadOnlyDictionary<string, T?> Data { get; set; }
    }
}