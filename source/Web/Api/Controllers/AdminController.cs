using AspNetSkeleton.Core;
using System;
using AspNetSkeleton.Api.Contract;
using System.Threading.Tasks;
using AspNetSkeleton.Service.Contract;
using System.Threading;
using AspNetSkeleton.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using AspNetSkeleton.Common;

namespace AspNetSkeleton.Api.Controllers
{
    [Route("[controller]/[action]")]
    [Authorize(Roles = BaseConstants.AdminRole)]
    public class AdminController : Controller
    {
        readonly IQueryDispatcher _queryDispatcher;
        readonly ICommandDispatcher _commandDispatcher;
        readonly IModelMetadataProvider _modelMetadataProvider;
        readonly MvcOptions _mvcOptions;

        public AdminController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher,
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
            var type = Service.Contract.Query.GetTypeBy(typeName);
            if (type == null)
                throw new HttpResponseException(StatusCodes.Status501NotImplemented);

            var formatterResult = await DeserializeBodyAsync(type);

            var query = (IQuery)formatterResult.Model;

            try
            {
                return await _queryDispatcher.DispatchAsync(query, cancellationToken);
            }
            catch (ServiceErrorException ex)
            {
                throw new ApiErrorException(ApiErrorCode.InvalidRequest, ex.Message, ex.Error);
            }
        }

        [HttpPost]
        public async Task<object> Command([FromQuery(Name = "t")]string typeName, CancellationToken cancellationToken)
        {
            var type = Service.Contract.Command.GetTypeBy(typeName);
            if (type == null)
                throw new HttpResponseException(StatusCodes.Status501NotImplemented);

            var formatterResult = await DeserializeBodyAsync(type);

            var command = (ICommand)formatterResult.Model;

            Polymorph<object> key = default;
            if (command is IKeyGeneratorCommand keyGeneratorCommand)
                keyGeneratorCommand.OnKeyGenerated = (c, k) => key = k;

            try
            {
                await _commandDispatcher.DispatchAsync(command, cancellationToken);
            }
            catch (ServiceErrorException ex)
            {
                throw new ApiErrorException(ApiErrorCode.InvalidRequest, ex.Message, ex.Error);
            }

            return key;
        }
    }
}
