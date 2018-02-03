using System;
using System.Collections.Generic;
using System.IO;
using AspNetSkeleton.Base;
using AspNetSkeleton.Common;
using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.Core.Hosting.Operations;
using AspNetSkeleton.Core.Infrastructure;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AspNetSkeleton.Core.Hosting
{
    public interface IHost : IOperationHost
    {
        IAppScope CreateAppScope();
    }

    public abstract class HostBase : OperationHost, IHost
    {
        public HostBase(IEnumerable<OperationDescriptor> operationDescriptors, IComponentContext context)
            : base(operationDescriptors, context.Resolve<IOperationHostIO>())
        {
            Environment = context.Resolve<IAppEnvironment>();
        }

        protected override string DefaultOperationName => ConsoleOperation.Name;

        public override string AppName => Environment.AppName;

        protected IAppEnvironment Environment { get; }

        public abstract IAppScope CreateAppScope();
    }
}
