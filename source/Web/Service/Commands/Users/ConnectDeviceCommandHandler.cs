using AspNetSkeleton.Common;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.DataAccess.Entities;
using AspNetSkeleton.Service.Contract;
using System.Threading.Tasks;
using System.Threading;
using System.Data.Entity;

namespace AspNetSkeleton.Service.Commands.Users
{
    public class ConnectDeviceCommandHandler : ICommandHandler<ConnectDeviceCommand>
    {
        readonly ICommandContext _commandContext;
        readonly IClock _clock;

        public ConnectDeviceCommandHandler(ICommandContext commandContext, IClock clock)
        {
            _commandContext = commandContext;
            _clock = clock;
        }

        public async Task HandleAsync(ConnectDeviceCommand command, CancellationToken cancellationToken)
        {
            this.RequireSpecified(command.DeviceId, c => c.DeviceId);

            using (var scope = _commandContext.CreateDataAccessScope())
            {
                var profile = await scope.Context.GetByKeyAsync<Profile>(cancellationToken, command.UserId).ConfigureAwait(false);
                this.RequireExisting(profile, c => c.UserId);

                var device = await scope.Context.GetByKeyTrackingAsync<Device>(cancellationToken, command.UserId, command.DeviceId).ConfigureAwait(false);
                var now = _clock.UtcNow;
                if (device == null)
                {
                    var deviceCount = await scope.Context.Query<Device>().CountAsync(d => d.UserId == command.UserId, cancellationToken).ConfigureAwait(false);
                    this.Require(profile.DeviceLimit == 0 || deviceCount < profile.DeviceLimit, CommandErrorCode.DeviceLimitExceeded);

                    device = new Device
                    {
                        Profile = profile,
                        DeviceId = command.DeviceId,
                        DeviceName = command.DeviceName,
                        ConnectedAt = now,
                        UpdatedAt = now
                    };

                    scope.Context.Create(device);
                }
                else
                {
                    device.DeviceName = command.DeviceName;
                    device.UpdatedAt = now;

                    scope.Context.Update(device);
                }

                await scope.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
