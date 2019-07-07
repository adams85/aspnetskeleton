using System;
using System.Reflection;
using Autofac;
using Autofac.Features.Metadata;
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
        readonly KeyValuePair<Type, CommandInterceptorFactory>[] _interceptorFactories;

        public CommandDispatcher(ILifetimeScope lifetimeScope, IEnumerable<Meta<CommandInterceptorFactory, CommandInterceptorMetadata>> interceptorFactories)
        {
            _lifetimeScope = lifetimeScope;
            _interceptorFactories = interceptorFactories.Select(item => new KeyValuePair<Type, CommandInterceptorFactory>(item.Metadata.LimitType, item.Value)).ToArray();
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

            ICommandInterceptor interceptor = this;
            KeyValuePair<Type, CommandInterceptorFactory> interceptorFactory;
            for (var i = _interceptorFactories.Length - 1; i >= 0; i--)
                if ((interceptorFactory = _interceptorFactories[i]).Key.IsAssignableFrom(actualCommandType))
                    interceptor = interceptorFactory.Value(interceptor);

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

            var isNestedCommand = _lifetimeScope.Tag == ServiceHostCoreAppConfiguration.CommandLifetimeScopeTag;
            var commandLifetimeScope =
                !isNestedCommand ?
                _lifetimeScope.BeginLifetimeScope(ServiceHostCoreAppConfiguration.CommandLifetimeScopeTag) :
                _lifetimeScope;

            try
            {
                var task = (Task)invokeHandlerMethod.Invoke(null, new object[] { commandLifetimeScope, context.Command, cancellationToken });
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