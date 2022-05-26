using MediatR;
using TLio.Contracts;

namespace TLio.Notifications;

public class CommandExecutionNotPossible : INotification
{
    public CommandExecutionNotPossible(ICommand command, string executionStatusMessage)
    {
        throw new NotImplementedException();
    }
}