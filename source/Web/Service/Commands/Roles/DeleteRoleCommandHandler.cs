using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.DataAccess.Entities;
using AspNetSkeleton.Service.Transforms;
using System.Threading.Tasks;
using System.Threading;
using AspNetSkeleton.DataAccess;

namespace AspNetSkeleton.Service.Commands.Roles
{
    public class DeleteRoleCommandHandler : ICommandHandler<DeleteRoleCommand>
    {
        readonly ICommandContext _commandContext;

        public DeleteRoleCommandHandler(ICommandContext commandContext)
        {
            _commandContext = commandContext;
        }

        public async Task HandleAsync(DeleteRoleCommand command, CancellationToken cancellationToken)
        {
            this.RequireSpecified(command.RoleName, c => c.RoleName);

            using (var scope = _commandContext.CreateDataAccessScope())
            {
                var role = await scope.Context.Query<Role>().GetByNameAsync(command.RoleName, cancellationToken).ConfigureAwait(false);
                this.RequireExisting(role, c => c.RoleName);

                if (!command.DeletePopulatedRole)
                    this.RequireIndependent(
                        await scope.Context.Query<UserRole>().AnyAsync(ur => ur.RoleId == role.RoleId, cancellationToken).ConfigureAwait(false),
                        c => c.RoleName);

                scope.Context.Delete(role);

                await scope.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
