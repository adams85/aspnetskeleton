using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AspNetSkeleton.Core;
using AspNetSkeleton.Service.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetSkeleton.Service.Host.Controllers
{
    [Route("[action]")]
    public class ServiceController : Controller
    {
        readonly IQueryDispatcher _queryDispatcher;
        readonly ICommandDispatcher _commandDispatcher;
        readonly IModelMetadataProvider _modelMetadataProvider;
        readonly MvcOptions _mvcOptions;

        public ServiceController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher,
            IModelMetadataProvider modelMetadataProvider, IOptions<MvcOptions> mvcOptions)
        {
            _queryDispatcher = queryDispatcher;
            _commandDispatcher = commandDispatcher;
            _modelMetadataProvider = modelMetadataProvider;
            _mvcOptions = mvcOptions.Value;
        }

        async Task<InputFormatterResult> DeserializeBodyAsync(Type modelType)
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(modelType);
            var modelState = new ModelStateDictionary();
            var formatterContext = new InputFormatterContext(HttpContext, string.Empty, modelState, modelMetadata, (s, e) => new StreamReader(s, e));

            var formatter = _mvcOptions.InputFormatters.FirstOrDefault(f => f.CanRead(formatterContext));
            InputFormatterResult formatterResult;
            if (formatter == null || !(formatterResult = await formatter.ReadAsync(formatterContext)).IsModelSet)
                throw new HttpResponseException(StatusCodes.Status415UnsupportedMediaType);

            return formatterResult;
        }

        [HttpPost]
        public async Task<object> Query([FromQuery(Name = "t")]string typeName, CancellationToken cancellationToken)
        {
            var type = Contract.Query.GetTypeBy(typeName);
            if (type == null)
                throw new HttpResponseException(StatusCodes.Status501NotImplemented);

            var formatterResult = await DeserializeBodyAsync(type);

            var query = (IQuery)formatterResult.Model;

            return await _queryDispatcher.DispatchAsync(query, cancellationToken);
        }

        [HttpPost]
        public async Task<object> Command([FromQuery(Name = "t")]string typeName, CancellationToken cancellationToken)
        {
            var type = Contract.Command.GetTypeBy(typeName);
            if (type == null)
                throw new HttpResponseException(StatusCodes.Status501NotImplemented);

            var formatterResult = await DeserializeBodyAsync(type);

            var command = (ICommand)formatterResult.Model;

            object key = null;
            if (command is IKeyGeneratorCommand keyGeneratorCommand)
                keyGeneratorCommand.OnKeyGenerated = (c, k) => key = k;

            await _commandDispatcher.DispatchAsync(command, cancellationToken);

            return key;
        }
    }
}
