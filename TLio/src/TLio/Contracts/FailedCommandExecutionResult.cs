namespace TLio.Contracts
{
    public class FailedCommandExecutionResult<T> : CommandExecutionResult<T>
    {
        public FailedCommandExecutionResult(IReadOnlyDictionary<string, T?> data, List<string> errors) : base(data)
        {
           ExecutionErrors.AddRange(errors);
        }
        
        public FailedCommandExecutionResult(IReadOnlyDictionary<string, T?> data, string errorMessage) : base(data)
        {
            ExecutionErrors.Add(errorMessage);
        }
        
        public List<string> ExecutionErrors { get; set; } = new();

        public bool IsSuccessfullyExecuted => ExecutionErrors.Any();
    }
}