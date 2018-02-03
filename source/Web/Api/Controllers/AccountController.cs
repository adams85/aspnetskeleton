using AspNetSkeleton.Common;
using AspNetSkeleton.Service.Contract;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.Api.Contract;
using AspNetSkeleton.Api.Contract.DataTransfer;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AspNetSkeleton.Core.DataTransfer;
using AspNetSkeleton.Core;
using AspNetSkeleton.Core.Infrastructure.Security;
using AspNetSkeleton.Core.Utils;

namespace AspNetSkeleton.Api.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class AccountController : Controller
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

        async Task CheckDeviceAsync(ClaimsPrincipal currentPrincipal, string deviceName, CancellationToken cancellationToken)
        {
            try
            {
                await _commandDispatcher.DispatchAsync(new ConnectDeviceCommand
                {
                    UserId = currentPrincipal.GetUserId().Value,
                    DeviceId = currentPrincipal.GetDeviceId(),
                    DeviceName = deviceName
                }, cancellationToken);
            }
            catch (CommandErrorException ex) when (ex.ErrorCode == CommandErrorCode.DeviceLimitExceeded)
            {
                throw new ApiErrorException(ApiErrorCode.DeviceNotAllowed);
            }
        }

        [HttpGet]
        public async Task<AccountData> Get([FromQuery(Name = "d")] string deviceName, CancellationToken cancellationToken)
        {
            await CheckDeviceAsync(User, deviceName, cancellationToken);

            return new AccountData
            {
                // ...
            };
        }
    }
}