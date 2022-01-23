using System;
using Lio.Core;

namespace Lio.Commands
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

        protected override bool OnCanExecute(ExecutionContext context)
        {
            return Path != null && Value != null;
        }

        protected override IExecutionResult OnExecute(ExecutionContext context)
        {
            context.JournalData.Add("", "");
            var selectedPaths = context.SpecificMutator.GetPathsForSelectionPath(Path);

            selectedPaths.ForEach(p => AddValue(p, context));

            return new CommandExecutionResult();
        }

        private void AddValue(string path, ExecutionContext context)
        {
            var targetType = context.SpecificMutator.GetTargetType(path);
            if (targetType == TargetTypes.Array) AddItemToArray(path, context);
            throw new NotImplementedException();
        }

        public void AddItemToArray(string path, ExecutionContext context)
        {
            context.SpecificMutator.AddItemToArray(path, Value);
        }
    }
}