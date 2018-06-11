using System.Net;
using System.Threading;
using Karambolo.Common;
using System;
using AspNetSkeleton.Service.Contract;
using System.Threading.Tasks;
using AspNetSkeleton.Api.Contract;
using System.Runtime.ExceptionServices;

namespace AspNetSkeleton.AdminTools.Infrastructure
{
    public class ApiProxyQueryDispatcher : ApiService, IQueryDispatcher
    {
        readonly IApiOperationContext _context;

        public ApiProxyQueryDispatcher(IApiOperationContext context)
            : base(context.Settings.ApiUrl, EnumerableUtils.FromElement<Predicate<Type>>(ServiceContractTypes.DataObjectTypes.Contains))
        {
            _context = context;
        }

        public async Task<object> DispatchAsync(IQuery query, CancellationToken cancellationToken)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var useCredentials =
                _context.ApiCredentials != null ?
                true :
                _context.ApiAuthToken != null ?
                false :
                throw new InvalidOperationException("No API credentials are available.");

            var actualQueryType = Query.GetActualTypeFor(query.GetType());
            var interfaceType = Query.GetInterfaceTypeFor(actualQueryType);
            var resultType = interfaceType.GetGenericArguments()[0];

            var queryString = new { t = actualQueryType.Name };
            var invokeTask =
                useCredentials ?
                this.InvokeApiAsync(cancellationToken, resultType, WebRequestMethods.Http.Post, "Admin/Query",
                    _context.ApiCredentials, string.Empty, queryString, content: query) :
                this.InvokeApiAsync(cancellationToken, resultType, WebRequestMethods.Http.Post, "Admin/Query",
                    _context.ApiAuthToken, queryString, content: query);

            ApiResult<object> result;
            try
            {
                result = await invokeTask
                    .WithTimeout(_context.Settings.ApiTimeout)
                    .ConfigureAwait(false);

                _context.ApiCredentials = null;
                _context.ApiAuthToken = result.AuthToken;
            }
            catch (ApiErrorException ex)
            {
                _context.ApiCredentials = null;
                _context.ApiAuthToken = ex.AuthToken;

                ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _context.ApiCredentials = null;
                _context.ApiAuthToken = null;

                throw new UnauthorizedAccessException(useCredentials ? "API credentials are invalid." : "API authentication token has expired.", ex);
            }

            return result.Content;
        }

        public async Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
        {
            return (TResult)await DispatchAsync((IQuery)query, cancellationToken).ConfigureAwait(false);
        }
    }
}
