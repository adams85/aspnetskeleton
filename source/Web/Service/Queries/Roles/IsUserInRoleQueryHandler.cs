using System.Linq;
using AspNetSkeleton.Service.Transforms;
using AspNetSkeleton.Service.Contract.Queries;
using AspNetSkeleton.DataAccess.Entities;
using System.Threading.Tasks;
using System.Threading;
using System.Data.Entity;
using Karambolo.Common;

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

                linq = linq.Where(UserTransforms.GetFilterByNameWhere(query.UserName).Substitute((UserRole ur) => ur.User));
                linq = linq.Where(RoleTransforms.GetFilterByNameWhere(query.RoleName).Substitute((UserRole ur) => ur.Role));

                return await linq.AnyAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
