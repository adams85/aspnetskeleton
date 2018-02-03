using AspNetSkeleton.Common.Cli;
using Karambolo.Common.Logging;
using System.IO;
using System.Collections.Generic;

namespace AspNetSkeleton.DeployTools
{
    class Program : OperationHost
    {
        public static readonly string AssemblyName = typeof(Program).Assembly.GetName().Name;
        public static readonly string AssemblyPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);

        static int Main(string[] args)
        {
            return new Program().Execute(args);
        }

        public Program()
            : base(OperationDescriptor.Scan(typeof(Program).Assembly.GetTypes()), ConsoleHostIO.Instance)
        {
            Logger = new TraceSourceLogger(AppName);
        }

        public override string AppName => AssemblyName;

        protected override IEnumerable<string> GetInstructions()
        {
            yield return $"You may provide the path to the service host application by specifying /{DbOperation.ServiceHostPathOption}=<path-to-host> option.";
        }
    }
}
