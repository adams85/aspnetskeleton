using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AspNetSkeleton.Common.DataTransfer;
using Karambolo.Common;

namespace AspNetSkeleton.Common
{
    public static class CommonTypes
    {
        static CommonTypes()
        {
            var assemblyTypes = typeof(ErrorData).Assembly.GetTypes();

            DataObjectTypes = assemblyTypes
                .Where(t =>
                    t.IsClass && !t.IsAbstract && !t.HasAttribute<CompilerGeneratedAttribute>() &&
                    t.Namespace.StartsWith(typeof(ErrorData).Namespace))
                .ToHashSet();
        }

        public static readonly IReadOnlyCollection<Type> DataObjectTypes;
    }
}
