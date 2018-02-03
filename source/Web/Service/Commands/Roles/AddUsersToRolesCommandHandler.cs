using System.Linq;
using AspNetSkeleton.Service.Transforms;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.DataAccess.Entities;
using AspNetSkeleton.DataAccess;
using System.Threading.Tasks;
using System.Threading;
using Karambolo.Common;

namespace AspNetSkeleton.Service.Commands.Roles
{
    public class AddUsersToRolesCommandHandler : ICommandHandler<AddUsersToRolesCommand>
    {
        readonly ICommandContext _commandContext;

        public AddUsersToRolesCommandHandler(ICommandContext commandContext)
        {
            _commandContext = commandContext;
        }

        public async Task HandleAsync(AddUsersToRolesCommand command, CancellationToken cancellationToken)
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
                var userIds = await
                (
                    from u in scope.Context.Query<User>().Where(userWhereBuilder.Build())
                    select u.UserId
                ).ToArrayAsync(cancellationToken).ConfigureAwait(false);

                this.RequireValid(userIds.Length == command.UserNames.Length, c => c.UserNames);

                var roleIds = await
                (
                    from r in scope.Context.Query<Role>().Where(roleWhereBuilder.Build())
                    from ur in r.Users.DefaultIfEmpty()
                    group new UserRole { RoleId = r.RoleId, UserId = ur.UserId } by r.RoleId into g
                    select g
                ).ToArrayAsync(cancellationToken).ConfigureAwait(false);

                this.RequireValid(roleIds.Length == command.RoleNames.Length, c => c.RoleNames);

                foreach (var roleId in roleIds)
                    foreach (var userId in userIds)
                        if (!roleId.Any(r => r.UserId == userId))
                            scope.Context.Create(new UserRole { UserId = userId, RoleId = roleId.Key });

                await scope.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
