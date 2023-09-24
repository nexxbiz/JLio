using TLio.Contexts;
using TLio.Models;

namespace TLio.Contracts
{
    public interface ICommand<T>
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
        ICommandExecutionResult<T> ExecuteAsync(CommandExecutionContext<T> context);

        /// <summary>
        /// Indicates if the command can be executed
        /// </summary>
        ExecutionStatus CanExecute(CommandExecutionContext<T> context);
    }
}