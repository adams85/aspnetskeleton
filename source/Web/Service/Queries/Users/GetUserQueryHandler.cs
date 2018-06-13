using System;
using AspNetSkeleton.Service.Transforms;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.Service.Contract.Queries;
using AspNetSkeleton.DataAccess.Entities;
using System.Threading.Tasks;
using System.Threading;

namespace AspNetSkeleton.Service.Queries.Users
{
    public class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserData>
    {
        readonly IQueryContext _queryContext;

        public GetUserQueryHandler(IQueryContext queryContext)
        {
            _queryContext = queryContext;
        }

        public async Task<UserData> HandleAsync(GetUserQuery query, CancellationToken cancellationToken)
        {
            User user;
            using (var scope = _queryContext.CreateDataAccessScope())
            {
                switch (query.Identifier)
                {
                    case UserIdentifier.Id:
                        this.RequireValid(query.Key.Value is int, q => q.Key);
                        user = await scope.Context.GetByKeyAsync<User>(cancellationToken, query.Key.Value).ConfigureAwait(false);
                        break;
                    case UserIdentifier.Name:
                        this.RequireValid(query.Key.Value is string, q => q.Key);

                        var userName = (string)query.Key.Value;
                        this.RequireSpecified<GetUserQuery, UserData, object>(userName, q => q.Key);

                        user = await scope.Context.Query<User>().GetByNameAsync(userName, cancellationToken).ConfigureAwait(false);
                        break;
                    case UserIdentifier.Email:
                        this.RequireValid(query.Key.Value is string, q => q.Key);

                        var email = (string)query.Key.Value;
                        this.RequireSpecified<GetUserQuery, UserData, object>(email, q => q.Key);

                        user = await scope.Context.Query<User>().GetByEmailAsync(email, cancellationToken).ConfigureAwait(false);
                        break;
                    default:
                        this.RequireValid(false, q => q.Identifier);
                        throw new InvalidOperationException();
                }
            }

            return user?.ToData();
        }
    }
}
