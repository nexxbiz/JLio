namespace TLio.Contracts
{
    public abstract class CommandExecutionResult<T> : ICommandExecutionResult<T>
    {

        public CommandExecutionResult(IReadOnlyDictionary<string, T?> data)
        {
            Data = data;
        }
        
        public IReadOnlyDictionary<string, T?> Data { get; set; }

    }
}