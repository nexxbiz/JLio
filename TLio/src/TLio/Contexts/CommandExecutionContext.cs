using TLio.Contracts;

namespace TLio.Contexts
{
    public class CommandExecutionContext<T>
    {
        public ILibraryExecutionContext<T> ExecutionContext { get; }
        
        public T? Input { get; }

        public CommandExecutionContext(ILibraryExecutionContext<T> scriptExecutionContext, T? input)
        {
            Input = input;
            ExecutionContext = scriptExecutionContext;
        }
    }
}