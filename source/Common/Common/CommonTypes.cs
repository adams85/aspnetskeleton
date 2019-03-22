using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Karambolo.Common;
using Karambolo.Common.Collections;
using AspNetSkeleton.Common.DataTransfer;

namespace AspNetSkeleton.Common
{
    public static class CommonTypes
    {
        static CommonTypes()
        {
            var assemblyTypes = typeof(ErrorData).Assembly.GetTypes();

            DataObjectTypes = Enumerable.ToHashSet(assemblyTypes
                .Where(t =>
                    t.IsClass && !t.IsAbstract && !t.HasAttribute<CompilerGeneratedAttribute>() &&
                    t.Namespace.StartsWith(typeof(ErrorData).Namespace)));
        }

        public static readonly IReadOnlyCollection<Type> DataObjectTypes;
    }
}
