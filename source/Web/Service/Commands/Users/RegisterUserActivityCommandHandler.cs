using AspNetSkeleton.Common;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.DataAccess.Entities;
using AspNetSkeleton.Service.Contract;
using System.Threading.Tasks;
using System.Threading;
using AspNetSkeleton.Service.Transforms;

namespace AspNetSkeleton.Service.Commands.Users
{
    public class RegisterUserActivityCommandHandler : ICommandHandler<RegisterUserActivityCommand>
    {
        readonly ICommandContext _commandContext;
        readonly ICommandDispatcher _commandDispatcher;
        readonly IClock _clock;

        public RegisterUserActivityCommandHandler(ICommandContext commandContext, ICommandDispatcher commandDispatcher, IClock clock)
        {
            _commandContext = commandContext;
            _commandDispatcher = commandDispatcher;
            _clock = clock;
        }

        public async Task HandleAsync(RegisterUserActivityCommand command, CancellationToken cancellationToken)
        {
            this.RequireSpecified(command.UserName, c => c.UserName);

            using (var scope = _commandContext.CreateDataAccessScope())
            {
                var user = await scope.Context.QueryTracking<User>().GetByNameAsync(command.UserName, cancellationToken).ConfigureAwait(false);
                this.RequireExisting(user, c => c.UserName);

                var now = _clock.UtcNow;
                if (command.SuccessfulLogin == true)
                {
                    user.PasswordFailuresSinceLastSuccess = 0;
                    user.LastLoginDate = now;
                }
                else if (command.SuccessfulLogin == false)
                {
                    var failures = user.PasswordFailuresSinceLastSuccess;
                    if (failures < ServiceConstants.MaxInvalidPasswordAttempts)
                    {
                        user.PasswordFailuresSinceLastSuccess += 1;
                        user.LastPasswordFailureDate = now;
                    }
                    else
                    {
                        user.LastPasswordFailureDate = now;
                        user.LastLockoutDate = now;
                        user.IsLockedOut = true;

                        await _commandDispatcher.DispatchAsync(new UserLockedOutNotificationArgs
                        {
                            Name = user.Profile?.FirstName,
                            UserName = user.UserName,
                            Email = user.Email,
                        }.ToCommand(), cancellationToken).ConfigureAwait(false);
                    }
                }

                if (command.UIActivity)
                    user.LastActivityDate = now;

                scope.Context.Update(user);

                await scope.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
