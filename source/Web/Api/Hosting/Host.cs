using System;
using System.Collections.Generic;
using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.Core;
using AspNetSkeleton.Core.Hosting;
using Autofac;

namespace AspNetSkeleton.Api.Hosting
{
    public class Host : HostBase
    {
        readonly Func<IAppScope> _appScopeFactory;

        public Host(IEnumerable<OperationDescriptor> operationDescriptors, IComponentContext context)
            : base(operationDescriptors, context)
        {
            _appScopeFactory = context.Resolve<Func<IAppScope>>();
        }

        public override IAppScope CreateAppScope()
        {
            return _appScopeFactory();
        }
    }
}
