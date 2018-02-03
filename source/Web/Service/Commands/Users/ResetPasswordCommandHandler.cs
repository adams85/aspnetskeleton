using System;
using System.Data.Entity;
using AspNetSkeleton.Common;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.DataAccess.Entities;
using AspNetSkeleton.Service.Transforms;
using AspNetSkeleton.Service.Contract;
using System.Threading.Tasks;
using System.Threading;

namespace AspNetSkeleton.Service.Commands.Users
{
    public class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand>
    {
        readonly ICommandContext _commandContext;
        readonly ICommandDispatcher _commandDispatcher;
        readonly IClock _clock;

        public ResetPasswordCommandHandler(ICommandContext commandContext, ICommandDispatcher commandDispatcher, IClock clock)
        {
            _commandContext = commandContext;
            _commandDispatcher = commandDispatcher;
            _clock = clock;
        }

        public async Task HandleAsync(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            this.RequireSpecified(command.UserName, c => c.UserName);

            using (var scope = _commandContext.CreateDataAccessScope())
            {
                var user = await scope.Context.QueryTracking<User>().Include(u => u.Profile).GetByNameAsync(command.UserName, cancellationToken).ConfigureAwait(false);
                this.RequireExisting(user, c => c.UserName);

                user.Password = string.Empty;
                user.PasswordVerificationToken = Guid.NewGuid().ToString("N");
                user.PasswordVerificationTokenExpirationDate = _clock.UtcNow + command.TokenExpirationTimeSpan;

                scope.Context.Update(user);

                await _commandDispatcher.DispatchAsync(new PasswordResetNotificationArgs
                {
                    Name = user.Profile?.FirstName,
                    UserName = user.UserName,
                    Email = user.Email,
                    VerificationToken = user.PasswordVerificationToken,
                    VerificationTokenExpirationDate = user.PasswordVerificationTokenExpirationDate.Value,
                }.ToCommand(), cancellationToken).ConfigureAwait(false);

                await scope.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
