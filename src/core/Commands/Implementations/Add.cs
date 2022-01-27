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
            var selectedItems = context.ScriptExecutionContext.SpecificItemFetcher.GetItemsForPath(Path, context.Input);
            selectedItems.ForEach(i => AddValue(i, context.ScriptExecutionContext));
            return Success("");
        }

        private void AddValue(FetchedItem item, ScriptExecutionContext context)
        {
            if (item.ItemType == TargetTypes.Array) AddValueToArray(item, context);
            if (item.ItemType == TargetTypes.Object) AddValueToObject(item, context);
        }

        private void AddValueToObject(FetchedItem item, ScriptExecutionContext context)
        {
            context.SpecificMutator.AddValueToObject(item, Value);
        }

        public void AddValueToArray(FetchedItem item, ScriptExecutionContext context)
        {
            context.SpecificMutator.AddValueToArray(item, Value);
        }
    }
}