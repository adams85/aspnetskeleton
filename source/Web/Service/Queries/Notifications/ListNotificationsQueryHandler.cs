using System.Linq;
using AspNetSkeleton.Service.Transforms;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.Service.Contract.Queries;
using AspNetSkeleton.DataAccess.Entities;
using AspNetSkeleton.Service.Contract;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSkeleton.Service.Queries.Notifications
{
    public class ListNotificationsQueryHandler : ListQueryHandler<ListNotificationsQuery, NotificationData>
    {
        readonly IQueryContext _queryContext;

        public ListNotificationsQueryHandler(IQueryContext queryContext)
        {
            _queryContext = queryContext;
        }

        public override async Task<ListResult<NotificationData>> HandleAsync(ListNotificationsQuery query, CancellationToken cancellationToken)
        {
            Validate(query);

            using (var scope = _queryContext.CreateDataAccessScope())
            {
                var linq = scope.Context.Query<Notification>();

                if (query.State != null)
                    linq = linq.Where(m => m.State == query.State);

                return await ResultAsync(query, linq.ToData(), cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
