using TLio.Contexts;
using TLio.Contracts;

namespace TLio.Models
{
    public abstract class Command : ICommand
    {
        public Command() => Name = GetType().Namespace + GetType().Name;

        public string Id { get; set; } = default!;
        public string Name { get; set; }

        public abstract ICommandExecutionResult ExecuteAsync<T>(CommandExecutionContext<T> context);

        ExecutionStatus ICommand.CanExecute<T>(CommandExecutionContext<T> context) => CanExecute(context);

        public abstract ExecutionStatus CanExecute<T>(CommandExecutionContext<T> context);

        ICommandExecutionResult ICommand.ExecuteAsync<T>(CommandExecutionContext<T> context) => ExecuteAsync(context);
    }
}