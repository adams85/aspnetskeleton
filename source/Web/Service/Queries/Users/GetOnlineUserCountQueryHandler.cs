using AspNetSkeleton.Service.Contract.Queries;
using AspNetSkeleton.DataAccess.Entities;
using System.Threading.Tasks;
using System.Threading;
using System.Data.Entity;

namespace AspNetSkeleton.Service.Queries.Users
{
    public class GetOnlineUserCountQueryHandler : IQueryHandler<GetOnlineUserCountQuery, int>
    {
        readonly IQueryContext _queryContext;

        public GetOnlineUserCountQueryHandler(IQueryContext queryContext)
        {
            _queryContext = queryContext;
        }

        public async Task<int> HandleAsync(GetOnlineUserCountQuery query, CancellationToken cancellationToken)
        {
            using (var scope = _queryContext.CreateDataAccessScope())
            {
                return await scope.Context.Query<User>().CountAsync(u => u.LastActivityDate > query.DateFrom, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
