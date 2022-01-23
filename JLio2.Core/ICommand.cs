namespace Lio.Core
{
    public interface ICommand
    {
        string Name { get; }
        bool CanExecute(ExecutionContext executionContext);
        IExecutionResult Execute(ExecutionContext executionContext);
    }
}