using AspNetSkeleton.Common.Cli;
using Karambolo.Common.Logging;
using System.IO;

namespace AspNetSkeleton.POTools
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
    }
}
