using System;
using System.Linq;
using AspNetSkeleton.Common;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.DataAccess.Entities;
using AspNetSkeleton.Service.Transforms;
using AspNetSkeleton.Service.Contract;
using AspNetSkeleton.Common.Utils;
using System.Threading.Tasks;
using System.Threading;
using System.Data.Entity;
using AspNetSkeleton.Base.Utils;

namespace AspNetSkeleton.Service.Commands.Users
{
    public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
    {
        readonly ICommandContext _commandContext;
        readonly ICommandDispatcher _commandDispatcher;
        readonly IClock _clock;

        public CreateUserCommandHandler(ICommandContext commandContext, ICommandDispatcher commandDispatcher, IClock clock)
        {
            _commandContext = commandContext;
            _commandDispatcher = commandDispatcher;
            _clock = clock;
        }

        public async Task HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
        {
            this.RequireSpecified(command.UserName, c => c.UserName);
            this.RequireValid(command.UserName.IndexOf(',') < 0, c => c.UserName);

            this.RequireSpecified(command.Email, c => c.Email);
            this.RequireValid(ValidationUtils.IsValidEmailAddress(command.Email), c => c.Email);

            this.RequireSpecified(command.Password, c => c.Password);
            this.RequireValid(
                command.Password.Length >= ServiceConstants.MinRequiredPasswordLength &&
                command.Password.Count(c => !char.IsLetterOrDigit(c)) >= ServiceConstants.MinRequiredNonAlphanumericCharacters,
                c => c.Password);

            if (command.CreateProfile)
            {
                this.RequireSpecified(command.FirstName, c => c.FirstName);
                this.RequireSpecified(command.LastName, c => c.LastName);
            }

            using (var scope = _commandContext.CreateDataAccessScope())
            {
                this.RequireUnique(
                    await scope.Context.Query<User>().FilterByName(command.UserName).AnyAsync(cancellationToken).ConfigureAwait(false),
                    c => c.UserName);

                this.RequireUnique(
                    await scope.Context.Query<User>().FilterByEmail(command.Email).AnyAsync(cancellationToken).ConfigureAwait(false),
                    c => c.Email);

                var user = new User();

                user.UserName = command.UserName;
                user.Email = command.Email;
                user.Password = SecurityUtils.HashPassword(command.Password);
                user.IsApproved = command.IsApproved;
                if (!command.IsApproved)
                    user.ConfirmationToken = Guid.NewGuid().ToString("N");

                var now = _clock.UtcNow;
                user.CreateDate = now;
                user.LastPasswordChangedDate = now;
                user.PasswordFailuresSinceLastSuccess = 0;
                user.IsLockedOut = false;

                if (command.CreateProfile)
                {
                    user.Profile = new Profile();
                    user.Profile.FirstName = command.FirstName;
                    user.Profile.LastName = command.LastName;
                    user.Profile.PhoneNumber = command.PhoneNumber;
                    user.Profile.DeviceLimit = command.DeviceLimit;
                }

                scope.Context.Create(user);

                if (!command.IsApproved)
                    await _commandDispatcher.DispatchAsync(new UnapprovedUserCreatedNotificationArgs
                    {
                        Name = user.Profile?.FirstName,
                        UserName = user.UserName,
                        Email = user.Email,
                        VerificationToken = user.ConfirmationToken,
                    }.ToCommand(), cancellationToken).ConfigureAwait(false);

                await scope.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                this.RaiseKeyGenerated(command, user.UserId);
            }            
        }
    }
}
