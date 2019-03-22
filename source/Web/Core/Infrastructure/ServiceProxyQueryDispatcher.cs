using AspNetSkeleton.Common;
using System;
using System.Linq;
using System.Net;
using AspNetSkeleton.Common.DataTransfer;
using AspNetSkeleton.Service.Contract;
using System.Threading;
using Karambolo.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace AspNetSkeleton.Core.Infrastructure
{
    public class ServiceProxyQueryDispatcher : WebApiInvoker, IQueryDispatcher
    {
        readonly CoreSettings _settings;

        public ServiceProxyQueryDispatcher(IOptions<CoreSettings> settings) :
            base(settings.Value.ServiceBaseUrl, new Predicate<Type>[] { ServiceContractTypes.DataObjectTypes.Contains })
        {
            _settings = settings.Value;
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
