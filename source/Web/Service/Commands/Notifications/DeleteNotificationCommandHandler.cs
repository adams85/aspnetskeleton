using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.DataAccess.Entities;
using System.Threading.Tasks;
using System.Threading;

namespace AspNetSkeleton.Service.Commands.Notifications
{
    public class DeleteNotificationCommandHandler : ICommandHandler<DeleteNotificationCommand>
    {
        readonly ICommandContext _commandContext;

        public DeleteNotificationCommandHandler(ICommandContext commandContext)
        {
            _commandContext = commandContext;
        }

        public async Task HandleAsync(DeleteNotificationCommand command, CancellationToken cancellationToken)
        {
            using (var scope = _commandContext.CreateDataAccessScope())
            {
                var notification = await scope.Context.GetByKeyAsync<Notification>(cancellationToken, command.Id).ConfigureAwait(false);
                this.RequireExisting(notification, c => c.Id);

                scope.Context.Delete(notification);

                await scope.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
