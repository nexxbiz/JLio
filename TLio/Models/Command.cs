using System.Collections.Generic;
using TLio.Contexts;
using TLio.Contracts;

namespace TLio.Models
{
    public abstract class Command : ICommand
    {
        public Command() => Name = GetType().Namespace + GetType().Name;

        public string Id { get; set; } = default!;
        public string Name { get; set; }

        protected abstract IReadOnlyDictionary<string, object> ExecuteAsync(CommandExecutionContext context);

        ExecutionStatus ICommand.CanExecute(CommandExecutionContext context) => CanExecute(context);

        protected abstract ExecutionStatus CanExecute(CommandExecutionContext context);

        IReadOnlyDictionary<string, object> ICommand.ExecuteAsync(CommandExecutionContext context) => ExecuteAsync(context);
    }
}