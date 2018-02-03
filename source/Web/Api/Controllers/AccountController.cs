using System.Web.Http;
using AspNetSkeleton.Api.Infrastructure.Security;
using AspNetSkeleton.Common;
using AspNetSkeleton.Service.Contract;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.Api.Contract;
using AspNetSkeleton.Api.Contract.DataTransfer;
using System.Threading.Tasks;
using System.Threading;

namespace AspNetSkeleton.Api.Controllers
{
    [Authorize]
    public class AccountController : ApiController
    {
        readonly IQueryDispatcher _queryDispatcher;
        readonly ICommandDispatcher _commandDispatcher;
        readonly IClock _clock;

        public AccountController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, IClock clock)
        {
            _queryDispatcher = queryDispatcher;
            _commandDispatcher = commandDispatcher;
            _clock = clock;
        }

        async Task CheckDeviceAsync(ApiPrincipal currentPrincipal, string deviceName, CancellationToken cancellationToken)
        {
            try
            {
                await _commandDispatcher.DispatchAsync(new ConnectDeviceCommand
                {
                    UserId = currentPrincipal.AccountInfo.UserId,
                    DeviceId = currentPrincipal.DeviceId,
                    DeviceName = deviceName
                }, cancellationToken);
            }
            catch (CommandErrorException ex) when (ex.ErrorCode == CommandErrorCode.DeviceLimitExceeded)
            {
                throw new ApiErrorException(ApiErrorCode.DeviceNotAllowed);
            }
        }

        public async Task<AccountData> Get([FromUri] string deviceName, CancellationToken cancellationToken)
        {
            var currentPrincipal = (ApiPrincipal)RequestContext.Principal;
            await CheckDeviceAsync(currentPrincipal, deviceName, cancellationToken);

            return new AccountData
            {
                // ...
            };
        }
    }
}