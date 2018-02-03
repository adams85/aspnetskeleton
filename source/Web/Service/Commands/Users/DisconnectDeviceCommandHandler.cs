using AspNetSkeleton.Common;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.DataAccess.Entities;
using AspNetSkeleton.Service.Contract;
using System.Threading.Tasks;
using System.Threading;

namespace AspNetSkeleton.Service.Commands.Users
{
    public class DisconnectDeviceCommandHandler : ICommandHandler<DisconnectDeviceCommand>
    {
        readonly ICommandContext _commandContext;
        readonly IClock _clock;

        public DisconnectDeviceCommandHandler(ICommandContext commandContext, IClock clock)
        {
            _commandContext = commandContext;
            _clock = clock;
        }

        public async Task HandleAsync(DisconnectDeviceCommand command, CancellationToken cancellationToken)
        {
            this.RequireSpecified(command.DeviceId, c => c.DeviceId);

            using (var scope = _commandContext.CreateDataAccessScope())
            {
                var profile = await scope.Context.GetByKeyAsync<Profile>(cancellationToken, command.UserId).ConfigureAwait(false);
                this.RequireExisting(profile, c => c.UserId);

                var device = await scope.Context.GetByKeyTrackingAsync<Device>(cancellationToken, command.UserId, command.DeviceId).ConfigureAwait(false);
                this.RequireExisting(device, c => c.DeviceId);

                this.Require(_clock.UtcNow - device.ConnectedAt >= command.DisconnectTimeSpan, CommandErrorCode.DeviceDisconnectTimeNotExpired);

                scope.Context.Delete(device);

                await scope.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
