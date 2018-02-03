using System.Linq;
using AspNetSkeleton.Service.Transforms;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.DataAccess.Entities;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Threading;
using Karambolo.Common;

namespace AspNetSkeleton.Service.Commands.Roles
{
    public class RemoveUsersFromRolesCommandHandler : ICommandHandler<RemoveUsersFromRolesCommand>
    {
        readonly ICommandContext _commandContext;

        public RemoveUsersFromRolesCommandHandler(ICommandContext commandContext)
        {
            _commandContext = commandContext;
        }

        public async Task HandleAsync(RemoveUsersFromRolesCommand command, CancellationToken cancellationToken)
        {
            this.RequireSpecified(command.UserNames, c => c.UserNames);
            this.RequireSpecified(command.RoleNames, c => c.RoleNames);

            var userWhereBuilder = PredicateBuilder<User>.False();
            foreach (var userName in command.UserNames)
            {
                this.RequireValid(userName != null, c => c.UserNames);
                userWhereBuilder.Or(UserTransforms.GetFilterByNameWhere(userName));
            }

            var roleWhereBuilder = PredicateBuilder<Role>.False();
            foreach (var roleName in command.RoleNames)
            {
                this.RequireValid(roleName != null, c => c.RoleNames);
                roleWhereBuilder.Or(RoleTransforms.GetFilterByNameWhere(roleName));
            }

            using (var scope = _commandContext.CreateDataAccessScope())
            {
                var users = await scope.Context.QueryTracking<User>().Where(userWhereBuilder.Build())
                    .ToArrayAsync(cancellationToken).ConfigureAwait(false);
                this.RequireValid(users.Length == command.UserNames.Length, c => c.UserNames);

                var roles = await scope.Context.QueryTracking<Role>().Include(r => r.Users).Where(roleWhereBuilder.Build())
                    .ToArrayAsync(cancellationToken).ConfigureAwait(false);
                this.RequireValid(roles.Length == command.RoleNames.Length, c => c.RoleNames);

                foreach (var role in roles)
                    foreach (var user in users)
                    {
                        var userRole = role.Users.FirstOrDefault(ur => ur.UserId == user.UserId);
                        if (userRole != null)
                            role.Users.Remove(userRole);
                    }

                foreach (var role in roles)
                    scope.Context.Update(role);

                await scope.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
