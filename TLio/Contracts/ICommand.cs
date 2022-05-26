using TLio.Contexts;
using TLio.Models;

namespace TLio.Contracts
{
    public interface ICommand
    {
        /// <summary>
        /// Unique identifier of the command within the script
        /// </summary>
        string Id { get; set; }
        
        /// <summary>
        /// The name of the command
        /// </summary>
        string Name { get; set; }
        
        /// <summary>
        /// Invoked when the command executes
        /// </summary>
        /// <param name="context"></param>
        ICommandExecutionResult ExecuteAsync(CommandExecutionContext context);

        /// <summary>
        /// Indicates if the command can be executed
        /// </summary>
        ExecutionStatus CanExecute(CommandExecutionContext context);
    }

    public abstract class CommandExecutionResult : ICommandExecutionResult
    {

        public CommandExecutionResult(IReadOnlyDictionary<string, object> data)
        {
            Data = data;
        }
        
        public IReadOnlyDictionary<string, object> Data { get; set; }

    }

    public class SuccessCommandExecutionResult : CommandExecutionResult
    {
        public SuccessCommandExecutionResult(IReadOnlyDictionary<string, object> data) : base(data)
        {
        }
    }
    
    public class FailedCommandExecutionResult : CommandExecutionResult
    {
        public FailedCommandExecutionResult(IReadOnlyDictionary<string, object> data, List<string> errors) : base(data)
        {
           ExecutionErrors.AddRange(errors);
        }
        
        public FailedCommandExecutionResult(IReadOnlyDictionary<string, object> data, string errorMessage) : base(data)
        {
            ExecutionErrors.Add(errorMessage);
        }
        
        public List<string> ExecutionErrors { get; set; } = new();

        public bool IsSuccessfullyExecuted => ExecutionErrors.Any();
    }
    
    public interface ICommandExecutionResult
    {
        public IReadOnlyDictionary<string, object> Data { get; set; }
    }
    
    
}