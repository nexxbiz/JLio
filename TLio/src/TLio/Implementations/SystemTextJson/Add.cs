using System.Text.Json;
using System.Text.Json.Nodes;
using TLio.Contexts;
using TLio.Contracts;
using TLio.Models;
using TLio.Services.DataFetcher;

namespace TLio.Implementations.SystemTextJson
{
    public class Add : Command
    {

      
        public Add(string path, JsonNode value)
        {
            Path = path;
            Value = value;
        }

        public string Path { get; }

        public JsonNode? Value { get; }

        public override ExecutionStatus CanExecute<JsonNode>(CommandExecutionContext<JsonNode> context)
        {
            //TODO : evaluate if path is valid
            return new ExecutionStatus
            {
                CanExecute = Path != null && Value != null,
                Message = "Path and value are required"
            };
        }

        public override CommandExecutionResult ExecuteAsync<JsonNode>(CommandExecutionContext<JsonNode> context)
        {
            var selectedItems = context.ExecutionContext.DataFetcher.GetItemsForParentPath(Path, context.Input);

            var propertyName = Path.Substring(Path.LastIndexOf('.') + 1);
            selectedItems.ForEach(i => AddValue(i, context.ExecutionContext, propertyName));


            var result = new Dictionary<string, object>();

            return new SuccessCommandExecutionResult(context.ExecutionContext.DataFetcher.GetExecutionResult(context.Input));
        }


        private void AddValue(FetchedItem item, ILibraryExecutionContext context, string? propertyName = default)
        {
            if (item.ItemType == TargetTypes.Array) AddValueToArray(item, context);
            if (item.ItemType == TargetTypes.Object) AddValueToObject(item, context, propertyName);
        }

        private void AddValueToObject(FetchedItem item, ILibraryExecutionContext context, string propertyName)
        {
            context.Mutator.AddValueToObject(item, Value, propertyName);
        }

        public void AddValueToArray(FetchedItem item, ILibraryExecutionContext context)
        {
            context.Mutator.AddValueToArray(item, Value);
        }
    }
}