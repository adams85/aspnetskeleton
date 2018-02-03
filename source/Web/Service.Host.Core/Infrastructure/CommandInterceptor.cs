using AspNetSkeleton.Service.Contract;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSkeleton.Service.Host.Core.Infrastructure
{
    public class CommandInterceptorContext
    {
        public ICommand Command { get; set; }
        public Type CommandType { get; set; }

        public bool TryGet<TCommand>(out TCommand command)
            where TCommand : ICommand
        {
            if (CommandType == typeof(TCommand))
            {
                command = (TCommand)Command;
                return true;
            }
            else
            {
                command = default(TCommand);
                return false;
            }
        }
    }

    public interface ICommandInterceptor
    {
        Task ExecuteAsync(CommandInterceptorContext context, CancellationToken cancellationToken);
    }

    public delegate ICommandInterceptor CommandInterceptorFactory(ICommandInterceptor target);

    public abstract class CommandInterceptor : ICommandInterceptor
    {
        protected CommandInterceptor(ICommandInterceptor target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            Target = target;
        }

        protected ICommandInterceptor Target { get; }

        public abstract Task ExecuteAsync(CommandInterceptorContext context, CancellationToken cancellationToken);
    }
}
