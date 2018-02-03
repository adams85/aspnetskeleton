using System.Linq;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.DataAccess.Entities;
using System.Threading.Tasks;
using System.Threading;
using System.Data.Entity;

namespace AspNetSkeleton.Service.Commands.Notifications
{
    public class MarkNotificationsCommandHandler : ICommandHandler<MarkNotificationsCommand>
    {
        readonly ICommandContext _commandContext;

        public MarkNotificationsCommandHandler(ICommandContext commandContext)
        {
            _commandContext = commandContext;
        }

        public async Task HandleAsync(MarkNotificationsCommand command, CancellationToken cancellationToken)
        {
            using (var scope = _commandContext.CreateDataAccessScope())
            {
                var linq = scope.Context.QueryTracking<Notification>().Where(m => m.State != command.State);

                if (command.Count != null)
                    linq = linq.Take(command.Count.Value);

                var notifications = await linq.OrderBy(m => m.CreatedAt).ToArrayAsync(cancellationToken).ConfigureAwait(false);

                foreach (var notification in notifications)
                {
                    notification.State = command.State;
                    scope.Context.Update(notification);
                }

                await scope.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
