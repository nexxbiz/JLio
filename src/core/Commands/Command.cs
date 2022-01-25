using System;
using System.Collections.Generic;
using Lio.Core.Contexts;
using Lio.Core.Contracts;
using Lio.Core.ExecutionResult;

namespace Lio.Core.Commands
{
    public abstract class Command : ICommand
    {
        public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public string Name { get; set; }
        
        public Type Type => GetType();

        public bool CanExecute(CommandExecutionContext executionContext) => OnCanExecute(executionContext);

        public virtual ICommandExecutionResult Execute(CommandExecutionContext context) => OnExecute(context);
        protected virtual ICommandExecutionResult OnExecute(CommandExecutionContext context) => OnExecute();

        protected virtual ICommandExecutionResult OnExecute() => Success();

        protected virtual bool OnCanExecuteAsync(CommandExecutionContext context) => OnCanExecute(context);

        protected virtual bool OnCanExecute(CommandExecutionContext context) => true;

        protected virtual FaultCommandResult Error(Exception exception) => new FaultCommandResult(exception);
        protected virtual FaultCommandResult Error(string message) => new FaultCommandResult(message);
        protected virtual FaultCommandResult Fault(Exception exception) => new FaultCommandResult(exception);
        protected virtual FaultCommandResult Fault(string message) => new FaultCommandResult(message);
        protected virtual SuccessCommandResult Success(object? output = null) => new SuccessCommandResult(output);
    }
}