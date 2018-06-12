using System;
using System.Web.Http;
using AspNetSkeleton.Api.Contract;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using AspNetSkeleton.Service.Contract;
using System.Net;
using System.Threading;
using AspNetSkeleton.Base;
using AspNetSkeleton.Common;

namespace AspNetSkeleton.Api.Controllers
{
    [Authorize(Roles = BaseConstants.AdminRole)]
    public class AdminController : ApiController
    {
        readonly IQueryDispatcher _queryDispatcher;
        readonly ICommandDispatcher _commandDispatcher;

        public AdminController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher)
        {
            _queryDispatcher = queryDispatcher;
            _commandDispatcher = commandDispatcher;
        }

        async Task<object> DeserializeContentAsync(Type type, MediaTypeFormatter formatter)
        {
            using (var stream = await Request.Content.ReadAsStreamAsync().ConfigureAwait(false))
                return await formatter.ReadFromStreamAsync(type, stream, Request.Content, null).ConfigureAwait(false);
        }

        [HttpPost]
        public async Task<object> Query([FromUri(Name = "t")]string typeName, CancellationToken cancellationToken)
        {
            var type = Service.Contract.Query.GetTypeBy(typeName);
            if (type == null)
                throw new HttpResponseException(HttpStatusCode.NotImplemented);

            var formatter = Configuration.Formatters.FindReader(type, Request.Content.Headers.ContentType);
            object query;
            if (formatter == null ||
                (query = await DeserializeContentAsync(type, formatter)) == null)
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            try
            {
                return await _queryDispatcher.DispatchAsync((IQuery)query, cancellationToken);
            }
            catch (ServiceErrorException ex)
            {
                throw new ApiErrorException(ApiErrorCode.InvalidRequest, ex.Message, ex.Error);
            }
        }

        [HttpPost]
        public async Task<object> Command([FromUri(Name = "t")]string typeName, CancellationToken cancellationToken)
        {
            var type = Service.Contract.Command.GetTypeBy(typeName);
            if (type == null)
                throw new HttpResponseException(HttpStatusCode.NotImplemented);

            var formatter = Configuration.Formatters.FindReader(type, Request.Content.Headers.ContentType);
            object command;
            if (formatter == null ||
                (command = await DeserializeContentAsync(type, formatter)) == null)
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var key = default(Polymorph<object>);
            if (command is IKeyGeneratorCommand keyGeneratorCommand)
                keyGeneratorCommand.OnKeyGenerated = (c, k) => key = k;

            try
            {
                await _commandDispatcher.DispatchAsync((ICommand)command, cancellationToken);
            }
            catch (ServiceErrorException ex)
            {
                throw new ApiErrorException(ApiErrorCode.InvalidRequest, ex.Message, ex.Error);
            }

            return key;
        }
    }
}
