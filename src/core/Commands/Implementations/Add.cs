using Lio.Core.Contexts;
using Lio.Core.ExecutionResult;
using Lio.Core.Models;

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
            var selectedPaths = context.ScriptExecutionContext.SpecificMutator.GetItemsForSelectionPath(Path);
            selectedPaths.ForEach(p => AddValue(p, context.ScriptExecutionContext));
            return Success("");
        }

        private void AddValue(FetchedItem item, ScriptExecutionContext context)
        {
            if (item.TargetType == TargetTypes.Array) AddItemToArray(item, context);
            if (item.TargetType == TargetTypes.Object) AddItemToObject(item, context);
        }

        private void AddItemToObject(FetchedItem item, ScriptExecutionContext context)
        {
            context.SpecificMutator.AddItemToObject(item, Value);
        }

        public void AddItemToArray(FetchedItem item, ScriptExecutionContext context)
        {
            context.SpecificMutator.AddItemToArray(item, Value);
        }
    }
}