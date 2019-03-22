using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Karambolo.Common;
using Karambolo.Common.Collections;
using AspNetSkeleton.Api.Contract.DataTransfer;
using AspNetSkeleton.Common.DataTransfer;

namespace AspNetSkeleton.Api.Contract
{
    public static class ApiContractTypes
    {
        static ApiContractTypes()
        {
            var assemblyTypes = typeof(AccountData).Assembly.GetTypes();

            DataObjectTypes = Enumerable.ToHashSet(assemblyTypes
                .Where(t =>
                    t.IsClass && !t.IsAbstract && !t.HasAttribute<CompilerGeneratedAttribute>() &&
                    t.Namespace.StartsWith(typeof(AccountData).Namespace)));
        }

        public static readonly IReadOnlyCollection<Type> DataObjectTypes;
    }
}
