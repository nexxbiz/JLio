using MediatR;
using TLio.Contracts;

namespace TLio.Notifications;

public class CommandExecutionNotPossible<T> : INotification
{
    public CommandExecutionNotPossible(ICommand<T> command, string executionStatusMessage)
    {
        throw new NotImplementedException();
    }
}