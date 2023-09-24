using TLio.Contexts;
using TLio.Contracts;

namespace TLio.Models
{
    public abstract class Command<T> : ICommand<T>
    {
        public Command() => Name = GetType().Namespace + GetType().Name;

        public string Id { get; set; } = default!;
        public string Name { get; set; }

        public abstract ICommandExecutionResult<T> ExecuteAsync(CommandExecutionContext<T> context);

        public abstract ExecutionStatus CanExecute(CommandExecutionContext<T> context);
    }
}