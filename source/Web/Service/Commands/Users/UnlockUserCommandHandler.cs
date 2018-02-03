using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.DataAccess.Entities;
using AspNetSkeleton.Service.Transforms;
using System.Threading.Tasks;
using System.Threading;

namespace AspNetSkeleton.Service.Commands.Users
{
    public class UnlockUserCommandHandler : ICommandHandler<UnlockUserCommand>
    {
        readonly ICommandContext _commandContext;

        public UnlockUserCommandHandler(ICommandContext commandContext)
        {
            _commandContext = commandContext;
        }

        public async Task HandleAsync(UnlockUserCommand command, CancellationToken cancellationToken)
        {
            this.RequireSpecified(command.UserName, c => c.UserName);

            using (var scope = _commandContext.CreateDataAccessScope())
            {
                var user = await scope.Context.QueryTracking<User>().GetByNameAsync(command.UserName, cancellationToken).ConfigureAwait(false);
                this.RequireExisting(user, c => c.UserName);

                if (!user.IsLockedOut)
                    return;

                user.IsLockedOut = false;
                user.PasswordFailuresSinceLastSuccess = 0;

                scope.Context.Update(user);

                await scope.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
