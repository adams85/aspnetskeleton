using System;
using System.Web.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using AspNetSkeleton.Service.Contract;
using System.Net;
using System.Threading;
using AspNetSkeleton.Common;

namespace AspNetSkeleton.Service.Host.Controllers
{
    public class ServiceController : ApiController
    {
        readonly IQueryDispatcher _queryDispatcher;
        readonly ICommandDispatcher _commandDispatcher;

        public ServiceController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher)
        {
            _queryDispatcher = queryDispatcher;
            _commandDispatcher = commandDispatcher;
        }

        async Task<object> DeserializeBodyAsync(Type type, MediaTypeFormatter formatter)
        {
            using (var stream = await Request.Content.ReadAsStreamAsync().ConfigureAwait(false))
                try { return await formatter.ReadFromStreamAsync(type, stream, Request.Content, null).ConfigureAwait(false); }
                catch { return null; }
        }

        [HttpPost]
        public async Task<object> Query([FromUri(Name = "t")]string typeName, CancellationToken cancellationToken)
        {
            var type = Contract.Query.GetTypeBy(typeName);
            if (type == null)
                throw new HttpResponseException(HttpStatusCode.NotImplemented);

            var formatter = Configuration.Formatters.FindReader(type, Request.Content.Headers.ContentType);
            object query;
            if (formatter == null ||
                (query = await DeserializeBodyAsync(type, formatter)) == null)
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            return await _queryDispatcher.DispatchAsync((IQuery)query, cancellationToken);
        }

        [HttpPost]
        public async Task<object> Command([FromUri(Name = "t")]string typeName, CancellationToken cancellationToken)
        {
            var type = Contract.Command.GetTypeBy(typeName);
            if (type == null)
                throw new HttpResponseException(HttpStatusCode.NotImplemented);

            var formatter = Configuration.Formatters.FindReader(type, Request.Content.Headers.ContentType);
            object command;
            if (formatter == null ||
                (command = await DeserializeBodyAsync(type, formatter)) == null)
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var key = default(Polymorph<object>);
            if (command is IKeyGeneratorCommand keyGeneratorCommand)
                keyGeneratorCommand.OnKeyGenerated = (c, k) => key = k;

            await _commandDispatcher.DispatchAsync((ICommand)command, cancellationToken);

            return key;
        }
    }
}
