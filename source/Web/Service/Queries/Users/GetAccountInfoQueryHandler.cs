using System.Linq;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.Service.Contract.Queries;
using AspNetSkeleton.DataAccess.Entities;
using AspNetSkeleton.Service.Transforms;
using System.Threading.Tasks;
using System.Threading;
using AspNetSkeleton.DataAccess;

namespace AspNetSkeleton.Service.Queries.Users
{
    public class GetAccountInfoQueryHandler : IQueryHandler<GetAccountInfoQuery, AccountInfoData>
    {
        readonly IQueryContext _queryContext;

        public GetAccountInfoQueryHandler(IQueryContext queryContext)
        {
            _queryContext = queryContext;
        }

        public async Task<AccountInfoData> HandleAsync(GetAccountInfoQuery query, CancellationToken cancellationToken)
        {
            this.RequireSpecified(query.UserName, q => q.UserName);

            using (var scope = _queryContext.CreateDataAccessScope())
            {
                var linq =
                    from u in scope.Context.Query<User>().FilterByName(query.UserName)
                    select new AccountInfoData
                    {
                        UserId = u.UserId.Value,
                        UserName = u.UserName,
                        Email = u.Email,
                        FirstName = u.Profile != null ? u.Profile.FirstName : null,
                        LastName = u.Profile != null ? u.Profile.LastName : null,
                        LoginAllowed = u.IsApproved && !u.IsLockedOut
                    };

                var result = await linq.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

                if (result != null)
                {
                    var rolesLinq =
                        from u in scope.Context.Query<User>().Where(u => u.UserId.Value == result.UserId)
                        from ur in u.Roles
                        select ur.Role.RoleName;

                    result.Roles = await rolesLinq.ToArrayAsync(cancellationToken).ConfigureAwait(false);
                }

                return result;
            }
        }
    }
}
