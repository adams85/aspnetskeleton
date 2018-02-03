using System.Threading;
using System.Threading.Tasks;
using AspNetSkeleton.Base.Utils;
using AspNetSkeleton.DataAccess.Entities;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.Service.Contract.Queries;
using AspNetSkeleton.Service.Transforms;

namespace AspNetSkeleton.Service.Queries.Users
{
    public class AuthenticateUserQueryHandler : IQueryHandler<AuthenticateUserQuery, AuthenticateUserResult>
    {
        readonly IQueryContext _queryContext;

        public AuthenticateUserQueryHandler(IQueryContext queryContext)
        {
            _queryContext = queryContext;
        }

        public async Task<AuthenticateUserResult> HandleAsync(AuthenticateUserQuery query, CancellationToken cancellationToken)
        {
            this.RequireSpecified(query.UserName, q => q.UserName);
            this.RequireSpecified(query.Password, q => q.Password);

            User user;
            using (var scope = _queryContext.CreateDataAccessScope())
            {
                user = await scope.Context.Query<User>().GetByNameAsync(query.UserName, cancellationToken).ConfigureAwait(false);
            }

            var result = new AuthenticateUserResult();
            if (user == null)
                return result;

            result.UserId = user.UserId.Value;
            if (!user.IsApproved)
                result.Status = AuthenticateUserStatus.Unapproved;
            else if (user.IsLockedOut)
                result.Status = AuthenticateUserStatus.LockedOut;
            else
                result.Status =
                    user.Password != null && SecurityUtils.VerifyHashedPassword(user.Password, query.Password) ?
                    AuthenticateUserStatus.Successful :
                    AuthenticateUserStatus.Failed;

            return result;
        }
    }
}
