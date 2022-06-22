using TLio.Contexts;
using TLio.Contracts;
using TLio.Models;
using TLio.Services.DataFetcher;

namespace TLio.Implementations
{
    public class Add : Command
    {
        public Add(string path, IValue value)
        {
            Path = path;
            Value = value;
        }

        public string Path { get; }
        
        public IValue Value { get; }

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
            selectedItems.ForEach(i => AddValue(i, context.ScriptExecutionContext));
            return new SuccessCommandExecutionResult(new Dictionary<string, object>());
        }
        
        private void AddValue(FetchedItem item, ScriptExecutionContext context)
        {
            if (item.ItemType == TargetTypes.Array) AddValueToArray(item, context);
            if (item.ItemType == TargetTypes.Object) AddValueToObject(item, context);
        }
        
        private void AddValueToObject(FetchedItem item, ScriptExecutionContext context)
        {
            context.Mutator.AddValueToObject(item, Value);
        }
        
        public void AddValueToArray(FetchedItem item, ScriptExecutionContext context)
        {
            context.Mutator.AddValueToArray(item, Value);
        }
    }

    public interface IValue
    {
        
    }

    public class LiteralValue : IValue
    {
        public LiteralValue(object? value) => Value = value;
        public object Value { get; }
    }

    public interface IValueEvaluator
    {
        public object Evaluate(IValue expression);
    }

    public class LiteralValueEvaluator : IValueEvaluator
    {
        public object Evaluate(IValue expression)
        {
            var jsonExpression = (LiteralValue) expression;
            var value = jsonExpression.Value;
            return value;
        }
    }

    public class FunctionValue : IValue
    {
        public FunctionValue(string? value) => Value = value;
        public string Value { get; }
    }
}