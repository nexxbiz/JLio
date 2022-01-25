using System;
using Lio.Core.Contexts;
using Lio.Core.ExecutionResult;

namespace Lio.Core.Commands.Implementations
{
    public class Add : Command
    {
        public Add()
        {
        }

        public Add(string path, object value)
        {
            Path = path;
        }

        public Add(string path, IFunctionSupportedValue value)
        {
            Path = path;
            Value = value;
        }

        public string Path { get; set; }

        public IFunctionSupportedValue Value { get; set; }

        protected override bool OnCanExecute(CommandExecutionContext context)
        {
            return Path != null && Value != null;
        }

        protected override ICommandExecutionResult OnExecute(CommandExecutionContext context)
        {
            context.JournalData.Add("", "");
            var selectedPaths = context.ScriptExecutionContext.SpecificMutator.GetPathsForSelectionPath(Path);

            selectedPaths.ForEach(p => AddValue(p, context.ScriptExecutionContext));
            return Success("");
        }

        private void AddValue(string path, ScriptExecutionContext context)
        {
            var targetType = context.SpecificMutator.GetTargetType(path);
            if (targetType == TargetTypes.Array) AddItemToArray(path, context);
            throw new NotImplementedException();
        }

        public void AddItemToArray(string path, ScriptExecutionContext context)
        {
            context.SpecificMutator.AddItemToArray(path, Value);
        }
    }
}