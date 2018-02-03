using System.Collections.Generic;
using AspNetSkeleton.Base;
using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.Core.Hosting.Operations;
using Autofac;

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
