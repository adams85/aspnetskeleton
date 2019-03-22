using AspNetSkeleton.Common;
using System;
using System.Net;
using AspNetSkeleton.Common.DataTransfer;
using AspNetSkeleton.Service.Contract;
using System.Threading;
using Karambolo.Common;
using System.Threading.Tasks;
using System.Linq;

namespace AspNetSkeleton.Core.Infrastructure
{
    public class ServiceProxyQueryDispatcher : WebApiInvoker, IQueryDispatcher
    {
        readonly ICoreSettings _settings;

        public ServiceProxyQueryDispatcher(ICoreSettings settings) :
            base(settings.ServiceBaseUrl, new Predicate<Type>[] { ServiceContractTypes.DataObjectTypes.Contains })
        {
            _settings = settings;
        }

        protected override WebApiErrorException CreateError(WebHeaderCollection headers, ErrorData error)
        {
            return new QueryErrorException(error);
        }

        public async Task<object> DispatchAsync(IQuery query, CancellationToken cancellationToken)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var actualQueryType = Query.GetActualTypeFor(query.GetType());
            var interfaceType = Query.GetInterfaceTypeFor(actualQueryType);
            var resultType = interfaceType.GetGenericArguments()[0];

            var result = await InvokeAsync(cancellationToken, resultType,
                WebRequestMethods.Http.Post, "Query",
                query: new { t = actualQueryType.Name }, content: query)
                .WithTimeout(_settings.ServiceTimeOut)
                .ConfigureAwait(false);

            return result.Content;
        }

        public async Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
        {
            return (TResult)await DispatchAsync((IQuery)query, cancellationToken).ConfigureAwait(false);
        }
    }
}
