using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.DataAccess.Entities;
using AspNetSkeleton.Service.Transforms;
using AspNetSkeleton.Common;
using System.Threading.Tasks;
using System.Threading;

namespace AspNetSkeleton.Service.Commands.Users
{
    public class LockUserCommandHandler : ICommandHandler<LockUserCommand>
    {
        readonly ICommandContext _commandContext;
        readonly IClock _clock;

        public LockUserCommandHandler(ICommandContext commandContext, IClock clock)
        {
            _commandContext = commandContext;
            _clock = clock;
        }

        public async Task HandleAsync(LockUserCommand command, CancellationToken cancellationToken)
        {
            this.RequireSpecified(command.UserName, c => c.UserName);

            using (var scope = _commandContext.CreateDataAccessScope())
            {
                var user = await scope.Context.QueryTracking<User>().GetByNameAsync(command.UserName, cancellationToken).ConfigureAwait(false);
                this.RequireExisting(user, c => c.UserName);

                if (user.IsLockedOut)
                    return;

                user.LastLockoutDate = _clock.UtcNow;
                user.IsLockedOut = true;

                scope.Context.Update(user);

                await scope.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
