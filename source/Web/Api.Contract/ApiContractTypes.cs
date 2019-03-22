using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AspNetSkeleton.Api.Contract.DataTransfer;
using Karambolo.Common;

namespace AspNetSkeleton.Api.Contract
{
    public static class ApiContractTypes
    {
        static ApiContractTypes()
        {
            var assemblyTypes = typeof(AccountData).Assembly.GetTypes();

            DataObjectTypes = assemblyTypes
                .Where(t =>
                    t.IsClass && !t.IsAbstract && !t.HasAttribute<CompilerGeneratedAttribute>() &&
                    t.Namespace.StartsWith(typeof(AccountData).Namespace))
                .ToHashSet();
        }

        public static readonly IReadOnlyCollection<Type> DataObjectTypes;
    }
}
