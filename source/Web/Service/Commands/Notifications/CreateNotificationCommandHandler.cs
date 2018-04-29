using AspNetSkeleton.Common;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.DataAccess.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSkeleton.Service.Commands.Notifications
{
    public class CreateNotificationCommandHandler : ICommandHandler<CreateNotificationCommand>
    {
        readonly ICommandContext _commandContext;
        readonly IClock _clock;

        public CreateNotificationCommandHandler(ICommandContext commandContext, IClock clock)
        {
            _commandContext = commandContext;
            _clock = clock;
        }

        public async Task HandleAsync(CreateNotificationCommand command, CancellationToken cancellationToken)
        {
            this.RequireSpecified(command.Code, c => c.Code);

            using (var scope = _commandContext.CreateDataAccessScope())
            {
                var notification = new Notification();
                notification.State = NotificationState.Queued;
                notification.CreatedAt = _clock.UtcNow;
                notification.Code = command.Code;
                notification.Data = command.Data;

                var key = scope.Context.Create(notification);

                await scope.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                this.RaiseKeyGenerated(command, key);
            }
        }
    }
}
