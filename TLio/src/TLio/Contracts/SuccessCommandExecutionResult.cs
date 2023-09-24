namespace TLio.Contracts
{
    public class SuccessCommandExecutionResult<T> : CommandExecutionResult<T>
    {
        public SuccessCommandExecutionResult(IReadOnlyDictionary<string, T>? data) : base(data)
        {
        }
    }
}