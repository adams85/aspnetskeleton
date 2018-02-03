using AspNetSkeleton.Common.Infrastructure;
using Karambolo.Common;
using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using AspNetSkeleton.Common.Utils;

namespace AspNetSkeleton.Common.Cli
{
    public class OperationDescriptor
    {
        public static IEnumerable<OperationDescriptor> Scan(IEnumerable<Type> types)
        {
            return types
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Operation)))
                .Select(t => new OperationDescriptor(t))
                .Where(od => od.Name != null);
        }

        public OperationDescriptor(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!type.IsClass || type.IsAbstract || !type.IsSubclassOf(typeof(Operation)))
                throw new ArgumentException(null, nameof(type));

            Type = type;
            Name = type.GetAttributes<HandlerForAttribute>().FirstOrDefault()?.Key;
            Hint = type.GetAttributes<DisplayNameAttribute>().FirstOrDefault()?.DisplayName;
            Factory = (args, ctx) => (Operation)ExceptionUtils.UnwrapTargetInvocationException(() => Activator.CreateInstance(type, args, ctx));
        }

        public Type Type { get; }
        public string Name { get; set; }
        public string Hint { get; set; }
        public OperationFactory Factory { get; set; }
    }
}
