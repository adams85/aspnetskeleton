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
    public class ServiceProxyCommandDispatcher : WebApiInvoker, ICommandDispatcher
    {
        readonly ICoreSettings _settings;

        public ServiceProxyCommandDispatcher(ICoreSettings settings)
            : base(settings.ServiceBaseUrl, Enumerable.Empty<Predicate<Type>>())
        {
            _settings = settings;
        }

        protected override WebApiErrorException CreateError(WebHeaderCollection headers, ErrorData error)
        {
            return new CommandErrorException(error);
        }

        public async Task DispatchAsync(ICommand command, CancellationToken cancellationToken)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var actualCommandType = Command.GetActualTypeFor(command.GetType());

            var result = await InvokeAsync<Polymorph<object>>(cancellationToken,
                WebRequestMethods.Http.Post, "Command",
                query: new { t = actualCommandType.Name }, content: command)
                .WithTimeout(_settings.ServiceTimeOut)
                .ConfigureAwait(false);

            if (command is IKeyGeneratorCommand keyGeneratorCommand)
            {
                var key = result.Content;
                keyGeneratorCommand.OnKeyGenerated?.Invoke(command, key);
            }
        }
    }
}
