using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace TLio.Notifications.Implementations
{
    public class CommandExecutedNotificator : INotificationHandler<CommandExecuted>
    {
        public Task Handle(CommandExecuted notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}