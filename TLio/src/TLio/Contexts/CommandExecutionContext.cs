using TLio.Contracts;

namespace TLio.Contexts
{
    public class CommandExecutionContext<T>
    {
        public ILibraryExecutionContext ExecutionContext { get; }
        
        public T? Input { get; }

        public CommandExecutionContext(ILibraryExecutionContext scriptExecutionContext, T? input)
        {
            Input = input;
            ExecutionContext = scriptExecutionContext;
        }
    }
}