using System;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.DataAccess.Entities;
using AspNetSkeleton.Service.Transforms;
using System.Threading.Tasks;
using System.Threading;

namespace AspNetSkeleton.Service.Commands.Users
{
    public class ApproveUserCommandHandler : ICommandHandler<ApproveUserCommand>
    {
        readonly ICommandContext _commandContext;

        public ApproveUserCommandHandler(ICommandContext commandContext)
        {
            _commandContext = commandContext;
        }

        public async Task HandleAsync(ApproveUserCommand command, CancellationToken cancellationToken)
        {
            if (command.Verify)
                this.RequireSpecified(command.VerificationToken, c => c.VerificationToken);

            this.RequireSpecified(command.UserName, c => c.UserName);

            using (var scope = _commandContext.CreateDataAccessScope())
            {
                var user = await scope.Context.QueryTracking<User>().GetByNameAsync(command.UserName, cancellationToken).ConfigureAwait(false);
                this.RequireExisting(user, c => c.UserName);

                if (user.IsApproved)
                    return;

                if (command.Verify)
                    this.RequireValid(string.Equals(user.ConfirmationToken, command.VerificationToken, StringComparison.Ordinal), m => m.VerificationToken);

                user.ConfirmationToken = null;
                user.IsApproved = true;

                scope.Context.Update(user);

                await scope.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
