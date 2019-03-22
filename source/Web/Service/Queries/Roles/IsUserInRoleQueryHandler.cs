using System.Linq;
using AspNetSkeleton.Service.Transforms;
using AspNetSkeleton.Service.Contract.Queries;
using AspNetSkeleton.DataAccess.Entities;
using System.Threading.Tasks;
using System.Threading;
using AspNetSkeleton.DataAccess;
using Karambolo.Common;
using System;
using System.Linq.Expressions;

namespace AspNetSkeleton.Service.Queries.Roles
{
    public class IsUserInRoleQueryHandler : IQueryHandler<IsUserInRoleQuery, bool>
    {
        readonly IQueryContext _queryContext;

        public IsUserInRoleQueryHandler(IQueryContext queryContext)
        {
            _queryContext = queryContext;
        }

        public async Task<bool> HandleAsync(IsUserInRoleQuery query, CancellationToken cancellationToken)
        {
            this.RequireSpecified(query.UserName, q => q.UserName);
            this.RequireSpecified(query.RoleName, q => q.RoleName);

            using (var scope = _queryContext.CreateDataAccessScope())
            {
                var linq = scope.Context.Query<UserRole>();

                Expression<Func<UserRole, User>> selectUser = ur => ur.User;
                linq = linq.Where(selectUser.Chain(UserTransforms.GetFilterByNameWhere(query.UserName)));

                Expression<Func<UserRole, Role>> selectRole = ur => ur.Role;
                linq = linq.Where(selectRole.Chain(RoleTransforms.GetFilterByNameWhere(query.RoleName)));

                return await linq.AnyAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
