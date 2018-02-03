using System;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.Service.Contract.Queries;
using AspNetSkeleton.DataAccess.Entities;
using AspNetSkeleton.Service.Transforms;
using System.Threading.Tasks;
using System.Threading;

namespace AspNetSkeleton.Service.Queries.Roles
{
    public class GetRoleQueryHandler : IQueryHandler<GetRoleQuery, RoleData>
    {
        readonly IQueryContext _queryContext;

        public GetRoleQueryHandler(IQueryContext queryContext)
        {
            _queryContext = queryContext;
        }

        public async Task<RoleData> HandleAsync(GetRoleQuery query, CancellationToken cancellationToken)
        {
            Role role;

            using (var scope = _queryContext.CreateDataAccessScope())
            {
                switch (query.Identifier)
                {
                    case RoleIdentifier.Id:
                        this.RequireValid(query.Key is int, q => q.Key);
                        role = await scope.Context.GetByKeyAsync<Role>(cancellationToken, query.Key).ConfigureAwait(false);
                        break;
                    case RoleIdentifier.Name:
                        this.RequireValid(query.Key is string, q => q.Key);

                        var roleName = (string)query.Key;
                        this.RequireSpecified(roleName, q => q.Key);

                        role = await scope.Context.Query<Role>().GetByNameAsync(roleName, cancellationToken).ConfigureAwait(false);
                        break;
                    default:
                        this.RequireValid(false, q => q.Identifier);
                        throw new InvalidOperationException();
                }
            }

            return role.ToData();
        }
    }
}
