using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AspNetSkeleton.Common.DataTransfer;
using AspNetSkeleton.Common;
using AspNetSkeleton.Api.Contract.DataTransfer;
using System.Collections.Generic;
using Karambolo.Common;
using System.Linq;

namespace AspNetSkeleton.Api.Contract
{
    public class ApiResult<T> : WebApiResult<T>
    {
        public ApiResult(T content, string authToken) : base(content)
        {
            AuthToken = authToken;
        }
        
        public string AuthToken { get; private set; }
    }

    public interface IApiService
    {
        Task<ApiResult<object>> InvokeApiAsync(CancellationToken cancellationToken, Type responseType, string verb, string action,
            Action<WebHeaderCollection> headerSetter = null, object query = null, object content = null);

        Task<ApiResult<TResponse>> InvokeApiAsync<TResponse>(CancellationToken cancellationToken, string verb, string action,
            Action<WebHeaderCollection> headerSetter = null, object query = null, object content = null);
    }

    public class ApiService : WebApiInvoker, IApiService
    {
        public ApiService(string apiBaseUrl, IEnumerable<Predicate<Type>> typeFilters)
             : base(apiBaseUrl, Enumerable.Prepend(typeFilters, ApiContractTypes.DataObjectTypes.Contains)) { }

        protected override WebApiResult<TResponse> CreateResult<TResponse>(WebHeaderCollection headers, TResponse content)
        {
            return new ApiResult<TResponse>(content, headers[ApiContractConstants.AuthTokenHttpHeaderName]);
        }

        protected override WebApiErrorException CreateError(WebHeaderCollection headers, ErrorData error)
        {
            return new ApiErrorException(error, headers[ApiContractConstants.AuthTokenHttpHeaderName]);
        }

        public async Task<ApiResult<object>> InvokeApiAsync(CancellationToken cancellationToken, Type responseType, string verb, string action,
            Action<WebHeaderCollection> headerSetter = null, object query = null, object content = null)
        {
            return (ApiResult<object>)await InvokeAsync(cancellationToken, responseType, verb, action, headerSetter, query, content).ConfigureAwait(false);
        }

        public async Task<ApiResult<TResponse>> InvokeApiAsync<TResponse>(CancellationToken cancellationToken, string verb, string action,
            Action<WebHeaderCollection> headerSetter = null, object query = null, object content = null)
        {
            return (ApiResult<TResponse>)await InvokeAsync<TResponse>(cancellationToken, verb, action, headerSetter, query, content).ConfigureAwait(false);
        }
    }

    public static class ApiServiceExtensions
    {
        public static Task<ApiResult<object>> InvokeApiAsync(this IApiService @this, CancellationToken cancellationToken, Type responseType, string verb, string action,
            string authToken, object query = null, object content = null)
        {
            if (authToken == null)
                throw new ArgumentNullException(nameof(authToken));

            return @this.InvokeApiAsync(cancellationToken, responseType,
                verb, action,
                hc => hc[ApiContractConstants.AuthTokenHttpHeaderName] = authToken,
                query, content);
        }

        public static Task<ApiResult<TResponse>> InvokeApiAsync<TResponse>(this IApiService @this, CancellationToken cancellationToken, string verb, string action,
            string authToken, object query = null, object content = null)
        {
            if (authToken == null)
                throw new ArgumentNullException(nameof(authToken));

            return @this.InvokeApiAsync<TResponse>(cancellationToken,
                verb, action, 
                hc => hc[ApiContractConstants.AuthTokenHttpHeaderName] = authToken,
                query, content);
        }

        static string SerializeCredentials(string userName, string password, string deviceId)
        {
            var credentials = new CredentialsData
            {
                UserName = userName,
                Password = password,
                DeviceId = deviceId,
            };

            return CredentialsData.GenerateToken(credentials);
        }

        public static Task<ApiResult<object>> InvokeApiAsync(this IApiService @this, CancellationToken cancellationToken, Type responseType, string verb, string action,
            NetworkCredential credentials, string deviceId, object query = null, object content = null)
        {
            if (credentials == null)
                throw new ArgumentNullException(nameof(credentials));

            return @this.InvokeApiAsync(cancellationToken, responseType,
                verb, action,
                hc => hc[ApiContractConstants.CredentialsHttpHeaderName] = SerializeCredentials(credentials.UserName, credentials.Password, deviceId),
                query, content);
        }

        public static Task<ApiResult<TResponse>> InvokeApiAsync<TResponse>(this IApiService @this, CancellationToken cancellationToken, string verb, string action,
            NetworkCredential credentials, string deviceId, object query = null, object content = null)
        {
            if (credentials == null)
                throw new ArgumentNullException(nameof(credentials));

            return @this.InvokeApiAsync<TResponse>(cancellationToken,
                verb, action,
                hc => hc[ApiContractConstants.CredentialsHttpHeaderName] = SerializeCredentials(credentials.UserName, credentials.Password, deviceId),
                query, content);
        }
    }
}
