using AspNetSkeleton.Service.Contract;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.Service.Contract.Queries;
using Karambolo.Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Security;

namespace AspNetSkeleton.UI.Infrastructure.Security
{
    public interface IAccountManager
    {
        Task<AccountInfoData> GetAccountInfoAsync(string userName, bool registerActivity, CancellationToken cancellationToken);

        Task<bool> ValidateUserAsync(AuthenticateUserQuery query, CancellationToken cancellationToken);
        Task<MembershipCreateStatus> CreateUserAsync(CreateUserCommand command, CancellationToken cancellationToken);
        Task<bool> ChangePasswordAsync(string oldPassword, ChangePasswordCommand command, CancellationToken cancellationToken);
        Task VerifyUserAsync(ApproveUserCommand command, CancellationToken cancellationToken);
        Task ResetPasswordAsync(ResetPasswordCommand command, CancellationToken cancellationToken);
        Task SetPasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken);
    }

    public class AccountManager : IAccountManager
    {
        readonly IQueryDispatcher _queryDispatcher;
        readonly ICommandDispatcher _commandDispatcher;

        public AccountManager(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher)
        {
            _queryDispatcher = queryDispatcher;
            _commandDispatcher = commandDispatcher;
        }

        public async Task<AccountInfoData> GetAccountInfoAsync(string userName, bool registerActivity, CancellationToken cancellationToken)
        {
            var result = await _queryDispatcher.DispatchAsync(new GetAccountInfoQuery { UserName = userName }, cancellationToken).ConfigureAwait(false);

            if (result == null)
                return null;

            if (registerActivity)
                await _commandDispatcher.DispatchAsync(new RegisterUserActivityCommand
                {
                    UserName = result.UserName,
                    SuccessfulLogin = null,
                    UIActivity = true,
                }, CancellationToken.None).ConfigureAwait(false);

            return result;
        }

        public async Task<bool> ValidateUserAsync(AuthenticateUserQuery query, CancellationToken cancellationToken)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var authResult = await _queryDispatcher.DispatchAsync(query, cancellationToken).ConfigureAwait(false);
            if (authResult.UserId == null)
                return false;

            var success = authResult.Status == AuthenticateUserStatus.Successful;

            if (success || authResult.Status == AuthenticateUserStatus.Failed)
                await _commandDispatcher.DispatchAsync(new RegisterUserActivityCommand
                {
                    UserName = query.UserName,
                    SuccessfulLogin = success,
                    UIActivity = success,
                }, CancellationToken.None).ConfigureAwait(false);

            return success;
        }

        public async Task<MembershipCreateStatus> CreateUserAsync(CreateUserCommand command, CancellationToken cancellationToken)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            MembershipCreateStatus status;
            try
            {
                await _commandDispatcher.DispatchAsync(command, cancellationToken).ConfigureAwait(false);
                status = MembershipCreateStatus.Success;
            }
            catch (CommandErrorException ex)
            {
                string paramPath;
                switch (ex.ErrorCode)
                {
                    case CommandErrorCode.ParamNotSpecified:
                    case CommandErrorCode.ParamNotValid:
                        paramPath = (string)ex.Args[0];
                        status =
                            paramPath == Lambda.MemberPath((CreateUserCommand c) => c.UserName) ? MembershipCreateStatus.InvalidUserName :
                            paramPath == Lambda.MemberPath((CreateUserCommand c) => c.Email) ? MembershipCreateStatus.InvalidEmail :
                            paramPath == Lambda.MemberPath((CreateUserCommand c) => c.Password) ? MembershipCreateStatus.InvalidPassword :
                            MembershipCreateStatus.ProviderError;
                        break;
                    case CommandErrorCode.EntityNotUnique:
                        paramPath = (string)ex.Args[0];
                        status =
                            paramPath == Lambda.MemberPath((CreateUserCommand c) => c.UserName) ? MembershipCreateStatus.DuplicateUserName :
                            paramPath == Lambda.MemberPath((CreateUserCommand c) => c.Email) ? MembershipCreateStatus.DuplicateEmail :
                            MembershipCreateStatus.ProviderError;
                        break;
                    default:
                        status = MembershipCreateStatus.ProviderError;
                        break;
                }
            }

            return status;
        }

        public async Task<bool> ChangePasswordAsync(string oldPassword, ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            if (oldPassword == null)
                throw new ArgumentNullException(nameof(oldPassword));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (command.Verify)
                throw new ArgumentException(null, nameof(command));

            var authResult = await _queryDispatcher.DispatchAsync(new AuthenticateUserQuery
            {
                UserName = command.UserName,
                Password = oldPassword
            }, cancellationToken).ConfigureAwait(false);

            if (authResult.UserId == null)
                return false;

            var success = authResult.Status == AuthenticateUserStatus.Successful;

            if (success || authResult.Status == AuthenticateUserStatus.Failed)
                await _commandDispatcher.DispatchAsync(new RegisterUserActivityCommand
                {
                    UserName = command.UserName,
                    SuccessfulLogin = success,
                    UIActivity = success,
                }, CancellationToken.None).ConfigureAwait(false);

            if (success)
                await _commandDispatcher.DispatchAsync(command, cancellationToken).ConfigureAwait(false);

            return success;
        }

        public Task VerifyUserAsync(ApproveUserCommand command, CancellationToken cancellationToken)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (!command.Verify)
                throw new ArgumentException(null, nameof(command));

            return _commandDispatcher.DispatchAsync(command, cancellationToken);
        }

        public Task ResetPasswordAsync(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return _commandDispatcher.DispatchAsync(command, cancellationToken);
        }

        public Task SetPasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (!command.Verify)
                throw new ArgumentException(null, nameof(command));

            return _commandDispatcher.DispatchAsync(command, cancellationToken);
        }
    }
}