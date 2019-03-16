using System.Net;
using System.Threading;
using Karambolo.Common;
using System;
using AspNetSkeleton.Service.Contract;
using System.Threading.Tasks;
using AspNetSkeleton.Api.Contract;
using System.Runtime.ExceptionServices;
using AspNetSkeleton.Common;
using System.Linq;

namespace AspNetSkeleton.AdminTools.Infrastructure
{
    public class ApiProxyCommandDispatcher : ApiService, ICommandDispatcher
    {
        readonly IApiOperationContext _context;

        public ApiProxyCommandDispatcher(IApiOperationContext context)
            : base(context.Settings.ApiUrl, Enumerable.Empty<Predicate<Type>>())
        {
            _context = context;
        }

        public async Task DispatchAsync(ICommand command, CancellationToken cancellationToken)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var useCredentials =
                _context.ApiCredentials != null ? true :
                _context.ApiAuthToken != null ? false :
                throw new InvalidOperationException("No API credentials are available.");

            var actualCommandType = Command.GetActualTypeFor(command.GetType());

            var queryString = new { t = actualCommandType.Name };
            var invokeTask =
                useCredentials ?
                this.InvokeApiAsync<Polymorph<object>>(cancellationToken, WebRequestMethods.Http.Post, "Admin/Command",
                    _context.ApiCredentials, string.Empty, queryString, content: command) :
                this.InvokeApiAsync<Polymorph<object>>(cancellationToken, WebRequestMethods.Http.Post, "Admin/Command",
                    _context.ApiAuthToken, queryString, content: command);

            ApiResult<Polymorph<object>> result;
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

                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _context.ApiCredentials = null;
                _context.ApiAuthToken = null;

                throw new UnauthorizedAccessException(useCredentials ? "API credentials are invalid." : "API authentication token has expired.", ex);
            }

            if (command is IKeyGeneratorCommand keyGeneratorCommand)
            {
                var key = result.Content;
                keyGeneratorCommand.OnKeyGenerated?.Invoke(command, key);
            }
        }
    }
}
