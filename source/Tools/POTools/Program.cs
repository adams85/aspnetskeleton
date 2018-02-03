using AspNetSkeleton.Common.Cli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Karambolo.Extensions.Logging.File;
using Microsoft.Extensions.PlatformAbstractions;

namespace AspNetSkeleton.POTools
{
    class Program : OperationHost
    {
        public static readonly ApplicationEnvironment Environment = PlatformServices.Default.Application;

        public static IConfigurationRoot Configuration { get; private set; }

        static ILoggerFactory CreateLoggerFactory()
        {
            var result = new LoggerFactory();

            var config = Configuration.GetSection("Logging")?.GetSection(FileLoggerProvider.Alias);
            if (config != null)
                result.AddFile(new FileLoggerContext(Environment.ApplicationBasePath, "default.log"), config);

            return result;
        }

        static int Main(string[] args)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.ApplicationBasePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();

            using (var loggerFactory = CreateLoggerFactory())
                return new Program(loggerFactory).Execute(args);
        }

        public Program(ILoggerFactory loggerFactory) 
            : base(OperationDescriptor.Scan(typeof(Program).Assembly.GetTypes()), ConsoleHostIO.Instance)
        {
            Logger = loggerFactory.CreateLogger<Program>();
        }

        public override string AppName => Environment.ApplicationName;
    }
}
