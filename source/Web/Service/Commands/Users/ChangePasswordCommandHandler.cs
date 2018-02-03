using System;
using System.Linq;
using AspNetSkeleton.Common;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.DataAccess.Entities;
using AspNetSkeleton.Service.Transforms;
using System.Threading.Tasks;
using System.Threading;
using AspNetSkeleton.Base.Utils;

namespace AspNetSkeleton.Service.Commands.Users
{
    public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
    {
        readonly ICommandContext _commandContext;
        readonly IClock _clock;

        public ChangePasswordCommandHandler(ICommandContext commandContext, IClock clock)
        {
            _commandContext = commandContext;
            _clock = clock;
        }

        public async Task HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            if (command.Verify)
                this.RequireSpecified(command.VerificationToken, c => c.VerificationToken);

            this.RequireSpecified(command.UserName, c => c.UserName);

            this.RequireSpecified(command.NewPassword, c => c.NewPassword);
            this.RequireValid(
                command.NewPassword.Length >= ServiceConstants.MinRequiredPasswordLength &&
                command.NewPassword.Count(c => !char.IsLetterOrDigit(c)) >= ServiceConstants.MinRequiredNonAlphanumericCharacters,
                c => c.NewPassword);

            using (var scope = _commandContext.CreateDataAccessScope())
            {
                var user = await scope.Context.Query<User>().GetByNameAsync(command.UserName, cancellationToken).ConfigureAwait(false);
                this.RequireExisting(user, c => c.UserName);

                var now = _clock.UtcNow;
                if (command.Verify)
                {
                    this.RequireValid(
                        now < user.PasswordVerificationTokenExpirationDate && string.Equals(user.PasswordVerificationToken, command.VerificationToken, StringComparison.Ordinal),
                        c => c.VerificationToken);

                    scope.Context.Track(user);

                    user.PasswordVerificationToken = null;
                    user.PasswordVerificationTokenExpirationDate = null;

                    user.PasswordFailuresSinceLastSuccess = 0;
                    user.LastPasswordFailureDate = null;
                    user.IsLockedOut = false;
                }
                else
                    scope.Context.Track(user);

                user.Password = SecurityUtils.HashPassword(command.NewPassword);
                user.LastPasswordChangedDate = now;

                scope.Context.Update(user);

                await scope.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
