using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;
using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceProcess;

namespace AspNetSkeleton.Service.Host.Operations
{
    [HandlerFor(Name)]
    [DisplayName(Hint)]
    public class ServiceOperation : Operation
    {
        public const string Name = "service";
        public const string Hint = "Runs host as windows service. (Do not call directly!)";

        readonly ServiceBase _service;

        public ServiceOperation(string[] args, IOperationContext context, ServiceBase service) : base(args, context)
        {
            _service = service;
        }

        protected override int MandatoryArgCount => 0;

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name}";
            yield return Hint;
        }

        public override void Execute()
        {
            ServiceBase.Run(_service);
        }
    }
}
