using System;
using System.Reflection;
using Autofac;
using System.Linq;
using System.Collections.Generic;
using AspNetSkeleton.Service.Contract;
using AspNetSkeleton.Common.Infrastructure;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using Karambolo.Common;

namespace AspNetSkeleton.Service.Host.Core.Infrastructure
{
    public class CommandDispatcher : ICommandDispatcher, ICommandInterceptor
    {
        readonly ILifetimeScope _lifetimeScope;
        readonly IKeyedProvider<IEnumerable<CommandInterceptorFactory>> _interceptorFactoriesProvider;

        public CommandDispatcher(ILifetimeScope lifetimeScope, IKeyedProvider<IEnumerable<CommandInterceptorFactory>> interceptorFactoriesProvider)
        {
            _lifetimeScope = lifetimeScope;
            _interceptorFactoriesProvider = interceptorFactoriesProvider;
        }

        static readonly MethodInfo invokeHandlerMethodDefinition = Lambda.Method(() => InvokeHandlerAsync<ICommand>(null, null, default(CancellationToken))).GetGenericMethodDefinition();

        static Task InvokeHandlerAsync<TCommand>(ILifetimeScope lifetimeScope, TCommand command, CancellationToken cancellationToken)
            where TCommand : ICommand
        {
            var handler = lifetimeScope.Resolve<ICommandHandler<TCommand>>();
            return handler.HandleAsync(command, cancellationToken);
        }

        public Task DispatchAsync(ICommand command, CancellationToken cancellationToken)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var actualCommandType = Command.GetActualTypeFor(command.GetType());

            var interceptorFactories = _interceptorFactoriesProvider.ProvideFor(actualCommandType)
                .Concat(_interceptorFactoriesProvider.ProvideFor(KeyedProvider.Default));

            ICommandInterceptor interceptor = this;
            interceptor = interceptorFactories.Aggregate(interceptor, (ic, fac) => fac(ic));

            var context = new CommandInterceptorContext
            {
                Command = command,
                CommandType = actualCommandType,
            };

            return interceptor.ExecuteAsync(context, cancellationToken);
        }

        public async Task ExecuteAsync(CommandInterceptorContext context, CancellationToken cancellationToken)
        {
            var invokeHandlerMethod = invokeHandlerMethodDefinition.MakeGenericMethod(context.CommandType);

            var isNestedCommand = _lifetimeScope.Tag == ServiceHostCoreModule.CommandLifetimeScopeTag;
            var commandLifetimeScope =
                !isNestedCommand ?
                _lifetimeScope.BeginLifetimeScope(ServiceHostCoreModule.CommandLifetimeScopeTag) :
                _lifetimeScope;

            try
            {
                var task = (Task)invokeHandlerMethod.Invoke(null, ArrayUtils.FromElements<object>(commandLifetimeScope, context.Command, cancellationToken));
                await task.ConfigureAwait(false);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
            finally
            {
                if (!isNestedCommand)
                    commandLifetimeScope.Dispose();
            }
        }
    }
}