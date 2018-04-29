using System;
using System.Collections;
using System.Linq.Expressions;
using AspNetSkeleton.Service.Contract;
using System.Threading.Tasks;
using System.Threading;
using Karambolo.Common;
using AspNetSkeleton.DataAccess;

namespace AspNetSkeleton.Service
{
    public interface ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        Task HandleAsync(TCommand command, CancellationToken cancellationToken);
    }

    public static class CommandHandlerUtils
    {
        public static void Require<TCommand>(this ICommandHandler<TCommand> @this, bool condition, CommandErrorCode errorCode, Func<object[]> argsFactory = null)
            where TCommand : ICommand
        {
            if (!condition)
                throw new CommandErrorException(errorCode, (argsFactory ?? Default<object[]>.Func)());
        }

        public static void RequireSpecified<TCommand, T>(this ICommandHandler<TCommand> @this, T @param, Expression<Func<TCommand, T>> paramPath, bool emptyAllowed = false)
            where TCommand : ICommand
        {
            string paramString;
            ICollection paramCollection;
            @this.Require(
                @param != null &&
                (emptyAllowed || (paramString = @param as string) == null || paramString.Length > 0) &&
                (emptyAllowed || (paramCollection = @param as object[]) == null || paramCollection.Count > 0),
                CommandErrorCode.ParamNotSpecified, () => new[] { Lambda.PropertyPath(paramPath) });
        }

        public static void RequireValid<TCommand, T>(this ICommandHandler<TCommand> @this, bool condition, Expression<Func<TCommand, T>> paramPath)
            where TCommand : ICommand
        {
            @this.Require(condition, CommandErrorCode.ParamNotValid, () => new[] { Lambda.PropertyPath(paramPath) });
        }

        public static void RequireExisting<TCommand, T>(this ICommandHandler<TCommand> @this, object entity, Expression<Func<TCommand, T>> paramPath)
            where TCommand : ICommand
        {
            @this.Require(entity != null, CommandErrorCode.EntityNotFound, () => new[] { Lambda.PropertyPath(paramPath) });
        }

        public static void RequireUnique<TCommand, T>(this ICommandHandler<TCommand> @this, bool entityExists, Expression<Func<TCommand, T>> paramPath)
            where TCommand : ICommand
        {
            @this.Require(!entityExists, CommandErrorCode.EntityNotUnique, () => new[] { Lambda.PropertyPath(paramPath) });
        }

        public static void RequireIndependent<TCommand, T>(this ICommandHandler<TCommand> @this, bool entityHasDependencies, Expression<Func<TCommand, T>> paramPath)
            where TCommand : ICommand
        {
            @this.Require(!entityHasDependencies, CommandErrorCode.EntityDependent, () => new[] { Lambda.PropertyPath(paramPath) });
        }

        public static void RaiseKeyGenerated<TCommand>(this ICommandHandler<TCommand> @this, TCommand command, object keyValue)
            where TCommand : IKeyGeneratorCommand
        {
            if (command.OnKeyGenerated == null)
                return;

            if (keyValue is IdentityKey identityKey && identityKey.IsAvailable)
                keyValue = identityKey.ValueObject;

            command.OnKeyGenerated.Invoke(command, keyValue);
        }
    }
}
