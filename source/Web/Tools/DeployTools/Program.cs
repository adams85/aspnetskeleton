using System.Collections.Generic;
using AspNetSkeleton.Base;
using AspNetSkeleton.Common;
using AspNetSkeleton.Common.Cli;
using Karambolo.Extensions.Logging.File;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AspNetSkeleton.DeployTools
{
    class Program : OperationHost, IDbOperationContext
    {
        public static IConfigurationRoot Configuration { get; private set; }

        static ILoggerFactory CreateLoggerFactory()
        {
            var config = Configuration.GetSection("Logging");
            return new LoggerFactory()
                .AddFile(FileLoggerContext.Default, new ConfigurationFileLoggerSettings(config.GetSection(FileLoggerProvider.Alias),
                    o => o.FileAppender = new PhysicalFileAppender(AppEnvironment.Instance.AppBasePath));
        }

        static int Main(string[] args)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppEnvironment.Instance.AppBasePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
#if DISTRIBUTED
                .AddJsonFile("appsettings.Distributed.json", optional: true, reloadOnChange: false)
#else
                .AddJsonFile("appsettings.Monolithic.json", optional: true, reloadOnChange: false)
#endif
                .Build();

            using (var loggerFactory = CreateLoggerFactory())
               return new Program(loggerFactory).Execute(args);
        }

        public Program(ILoggerFactory loggerFactory)
            : base(OperationDescriptor.Scan(typeof(Program).Assembly.GetTypes()), ConsoleHostIO.Instance)
        {
            Logger = loggerFactory.CreateLogger<Program>();

            Clock = new Clock();
        }

        public override string AppName => AppEnvironment.Instance.AppName;

        public IClock Clock { get; }

        protected override IEnumerable<string> GetInstructions()
        {
            yield return $"You may provide the path to the service host application by specifying /{DbOperation.ServiceHostPathOption}=<path-to-host> option.";
        }
    }
}
