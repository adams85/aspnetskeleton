using System.Linq;
using AspNetSkeleton.Service.Transforms;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.Service.Contract.Queries;
using AspNetSkeleton.DataAccess.Entities;
using AspNetSkeleton.Service.Contract;
using System.Threading.Tasks;
using System.Threading;

namespace AspNetSkeleton.Service.Queries.Users
{
    public class ListUsersQueryHandler : ListQueryHandler<ListUsersQuery, UserData>
    {
        readonly IQueryContext _queryContext;

        public ListUsersQueryHandler(IQueryContext queryContext)
        {
            _queryContext = queryContext;
        }

        public override async Task<ListResult<UserData>> HandleAsync(ListUsersQuery query, CancellationToken cancellationToken)
        {
            Validate(query);

            using (var scope = _queryContext.CreateDataAccessScope())
            {
                var baseLinq =
                    query.RoleName != null ?
                    (
                        from r in scope.Context.Query<Role>().FilterByName(query.RoleName)
                        from ur in r.Users
                        select ur.User
                    ) :
                    scope.Context.Query<User>();

                if (query.UserNamePattern != null)
                    baseLinq = baseLinq.FilterByName(query.UserNamePattern, pattern: true);

                if (query.EmailPattern != null)
                    baseLinq = baseLinq.FilterByEmail(query.EmailPattern, pattern: true);

                var linq = Apply(query, baseLinq.ToData());

                return await ResultAsync(query, linq, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
