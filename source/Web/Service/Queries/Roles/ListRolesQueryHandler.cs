using System.Linq;
using AspNetSkeleton.Service.Transforms;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.Service.Contract.Queries;
using AspNetSkeleton.DataAccess.Entities;
using AspNetSkeleton.Service.Contract;
using System.Threading.Tasks;
using System.Threading;

namespace AspNetSkeleton.Service.Queries.Roles
{
    public class ListRolesQueryHandler : ListQueryHandler<ListRolesQuery, RoleData>
    {
        readonly IQueryContext _queryContext;

        public ListRolesQueryHandler(IQueryContext queryContext)
        {
            _queryContext = queryContext;
        }

        public override async Task<ListResult<RoleData>> HandleAsync(ListRolesQuery query, CancellationToken cancellationToken)
        {
            Validate(query);

            using (var scope = _queryContext.CreateDataAccessScope())
            {
                IQueryable<Role> linq;
                if (query.UserName != null)
                    linq =
                        from u in scope.Context.Query<User>().FilterByName(query.UserName)
                        from ur in u.Roles
                        select ur.Role;
                else
                    linq = scope.Context.Query<Role>();

                return await ResultAsync(query, linq.ToData(), cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
