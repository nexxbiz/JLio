using System;
using System.Collections.Generic;
using Lio.Core;

namespace Lio.Commands
{
    public abstract class Command : ICommand
    {
        public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public string Name { get; set; }
        public Type Type => GetType();

        public bool CanExecute(ExecutionContext executionContext)
        {
            return OnCanExecute(executionContext);
        }

        public IExecutionResult Execute(ExecutionContext executionContext)
        {
            return OnExecute(executionContext);
        }

        protected virtual IExecutionResult OnExecute(ExecutionContext context)
        {
            return OnExecute();
        }

        protected virtual IExecutionResult OnExecute()
        {
            return new ExecutionResult();
        }

        protected virtual bool OnCanExecute(ExecutionContext context)
        {
            return true;
        }
    }
}