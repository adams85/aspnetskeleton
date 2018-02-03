using System;
using System.Linq;
using AspNetSkeleton.DataAccess;
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
                var userWithProfile = await 
                (
                    from u in scope.Context.Query<User>().FilterByName(command.UserName)
                    select new { User = u, Profile = u.Profile }
                ).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

                this.RequireExisting(userWithProfile, c => c.UserName);

                var user = userWithProfile.User;

                scope.Context.Track(user);

                user.Password = string.Empty;
                user.PasswordVerificationToken = Guid.NewGuid().ToString("N");
                user.PasswordVerificationTokenExpirationDate = _clock.UtcNow + command.TokenExpirationTimeSpan;

                scope.Context.Update(user);

                await _commandDispatcher.DispatchAsync(new PasswordResetNotificationArgs
                {
                    Name = userWithProfile.Profile?.FirstName,
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
