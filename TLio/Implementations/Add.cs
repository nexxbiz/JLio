using TLio.Contexts;
using TLio.Contracts;
using TLio.Models;
using TLio.Services.DataFetcher;

namespace TLio.Implementations
{
    public class Add : Command
    {
        public Add(string path, object value)
        {
            Path = path;
            Value = value;
        }

        public string Path { get; }
        
        public object Value { get; }

        protected override ExecutionStatus CanExecute(CommandExecutionContext context)
        {
            //TODO : evaluate if path is valid
            return new ExecutionStatus
            {
                CanExecute = Path != null && Value != null,
                Message = "Path and value are required"
            };
        }

        protected override CommandExecutionResult ExecuteAsync(CommandExecutionContext context)
        {
            var selectedItems = context.ScriptExecutionContext.DataFetcher.GetItemsForParentPath(Path, context.Input);

            var propertyName = Path.Substring(Path.LastIndexOf('.') + 1);
            selectedItems.ForEach(i => AddValue(i, context.ScriptExecutionContext, propertyName));


            var result = new Dictionary<string, object>();

            return new SuccessCommandExecutionResult(context.ScriptExecutionContext.DataFetcher.GetExecutionResult(context.Input));
        }

        
        private void AddValue(FetchedItem item, ScriptExecutionContext context, string? propertyName = default)
        {
            if (item.ItemType == TargetTypes.Array) AddValueToArray(item, context);
            if (item.ItemType == TargetTypes.Object) AddValueToObject(item, context, propertyName);
        }
        
        private void AddValueToObject(FetchedItem item, ScriptExecutionContext context, string propertyName)
        {
            context.Mutator.AddValueToObject(item, Value, propertyName);
        }
        
        public void AddValueToArray(FetchedItem item, ScriptExecutionContext context)
        {
            context.Mutator.AddValueToArray(item, Value);
        }
    }
}