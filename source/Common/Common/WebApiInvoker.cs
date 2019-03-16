using AspNetSkeleton.Common.DataTransfer;
using AspNetSkeleton.Common.Utils;
using Karambolo.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSkeleton.Common
{
    public class WebApiResult<T>
    {
        public WebApiResult(T content)
        {
            Content = content;
        }

        public T Content { get; }
    }

    public class WebApiErrorException : ApplicationException
    {
        public WebApiErrorException(ErrorData error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            Error = error;
        }

        public ErrorData Error { get; }
        public object[] Args => Error.Args?.Select(a => a.Value).ToArray();

        public override string Message => $"Web API request failed with error code {Error?.Code.ToString() ?? "<unkown>"}.";
    }

    public class WebApiInvoker
    {
        const int bufferSize = 4096;

        readonly JsonSerializer _serializer;
        readonly string _apiBaseUrl;

        public WebApiInvoker(string apiBaseUrl, IEnumerable<Predicate<Type>> typeFilters)
        {
            _apiBaseUrl = apiBaseUrl;
            _serializer = JsonSerializer.Create(SerializationUtils.CreateDataTransferSerializerSettings(typeFilters.WithHead(CommonTypes.DataObjectTypes.Contains)));
        }

        protected virtual WebApiResult<TResponse> CreateResult<TResponse>(WebHeaderCollection headers, TResponse content)
        {
            return new WebApiResult<TResponse>(content);
        }

        protected virtual WebApiErrorException CreateError(WebHeaderCollection headers, ErrorData error)
        {
            return new WebApiErrorException(error);
        }

        async Task<TResult> InvokeAsync<TResult>(Type responseType, string verb, string action, Action<WebHeaderCollection> headerSetter, object query, object content,
            Func<WebHeaderCollection, object, TResult> resultFactory, CancellationToken cancellationToken)
        {
            var url = UriUtils.BuildUrl(new[] { _apiBaseUrl, action }, query);
            var webRequest = (HttpWebRequest)WebRequest.Create(url);

            headerSetter?.Invoke(webRequest.Headers);

            webRequest.KeepAlive = false;
            webRequest.ServicePoint.Expect100Continue = false;
            webRequest.Method = verb;
            webRequest.ContentType = webRequest.Accept = "application/json";

            if (verb != WebRequestMethods.Http.Head && verb != WebRequestMethods.Http.Get)
            {
                using (var memoryStream = new MemoryStream())
                using (var writer = new StreamWriter(memoryStream))
                {
                    _serializer.Serialize(writer, content);
                    writer.Flush();

                    memoryStream.Position = 0;
                    webRequest.ContentLength = memoryStream.Length;

                    using (var requestStream = await webRequest.GetRequestStreamAsync().AsCancellable(cancellationToken).ConfigureAwait(false))
                    {
                        await memoryStream.CopyToAsync(requestStream, bufferSize, cancellationToken).ConfigureAwait(false);
                        await requestStream.FlushAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            HttpWebResponse response;
            try { response = (HttpWebResponse)await webRequest.GetResponseAsync().AsCancellable(cancellationToken).ConfigureAwait(false); }
            catch (OperationCanceledException) { throw; }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError &&
                    (response = ex.Response as HttpWebResponse) != null)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                            break;
                        case HttpStatusCode.Unauthorized:
                            throw new UnauthorizedAccessException(null, ex);
                        case HttpStatusCode.NotImplemented:
                            throw new NotImplementedException(null, ex);
                        case HttpStatusCode.UnsupportedMediaType:
                            throw new FormatException(null, ex);
                        default:
                            throw;
                    }
                }
                else
                    throw;
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var responseStream = response.GetResponseStream())
                    await responseStream.CopyToAsync(memoryStream, bufferSize, cancellationToken).ConfigureAwait(false);

                memoryStream.Position = 0;

                using (var reader = new StreamReader(memoryStream))
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                        {
                            var resultContent = _serializer.Deserialize(reader, responseType);
                            return resultFactory(response.Headers, resultContent);
                        }
                        case HttpStatusCode.BadRequest:
                        {
                            var errorContent = (ErrorData)_serializer.Deserialize(reader, typeof(ErrorData));
                            throw CreateError(response.Headers, errorContent);
                        }
                        default:
                            throw new WebException(null, null, WebExceptionStatus.ProtocolError, response);
                    }
            }
        }

        public Task<WebApiResult<object>> InvokeAsync(CancellationToken cancellationToken, Type responseType, string verb, string action,
            Action<WebHeaderCollection> headerSetter = null, object query = null, object content = null)
        {
            if (responseType == null)
                throw new ArgumentNullException(nameof(responseType));

            return InvokeAsync(responseType, verb, action, headerSetter, query, content, CreateResult, cancellationToken);
        }

        public Task<WebApiResult<TResponse>> InvokeAsync<TResponse>(CancellationToken cancellationToken, string verb, string action,
            Action<WebHeaderCollection> headerSetter = null, object query = null, object content = null)
        {
            return InvokeAsync(typeof(TResponse), verb, action, headerSetter, query, content, (h, r) => CreateResult(h, (TResponse)r), cancellationToken);
        }
    }
}
