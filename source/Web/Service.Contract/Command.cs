using AspNetSkeleton.Common;
using AspNetSkeleton.Common.Infrastructure;
using Karambolo.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AspNetSkeleton.Service.Contract
{
    public interface ICommand { }

    public interface IKeyGeneratorCommand : ICommand
    {
        Action<ICommand, Polymorph<object>> OnKeyGenerated { get; set; }
    }

    public static class Command
    {
        static readonly Dictionary<string, Type> commandTypes = typeof(ICommand).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.HasInterface(typeof(ICommand)))
            .ToDictionary(t => t.Name, Identity<Type>.Func);

        static readonly ConcurrentDictionary<Type, Type> actualTypes = new ConcurrentDictionary<Type, Type>();

        public static Type GetTypeBy(string typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            return commandTypes.TryGetValue(typeName, out Type result) ? result : null;
        }

        public static Type GetActualTypeFor(Type commandType)
        {
            if (commandType == null)
                throw new ArgumentNullException(nameof(commandType));

            return actualTypes.GetOrAdd(commandType, t =>
            {
                if (!t.HasInterface(typeof(ICommand)))
                    throw new ArgumentException(nameof(commandType));

                var attribute = t.GetAttributes<HandledAsAttribute>().FirstOrDefault();
                Type actualType;
                if (attribute == null || (actualType = attribute.Type) == t)
                    return t;

                if (!t.IsSubclassOf(actualType))
                    throw new InvalidOperationException();

                return actualType;
            });
        }
    }
}
