using AspNetSkeleton.AdminTools.Infrastructure;
using AspNetSkeleton.Api.Contract;
using AspNetSkeleton.Common;
using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.Service.Contract;
using Karambolo.Extensions.Logging.File;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.Net;

namespace AspNetSkeleton.AdminTools
{
    class Program : OperationHost, IApiOperationContext
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
#if DISTRIBUTED
                .AddJsonFile("appsettings.Distributed.json", optional: true, reloadOnChange: false)
#else
                .AddJsonFile("appsettings.Monolithic.json", optional: true, reloadOnChange: false)
#endif
                .Build();

            using (var loggerFactory = CreateLoggerFactory())
                return new Program(loggerFactory).Execute(args);
        }

        readonly Lazy<IQueryDispatcher> _queryDispatcher;
        readonly Lazy<ICommandDispatcher> _commandDispatcher;

        public Program(ILoggerFactory loggerFactory)
            : base(OperationDescriptor.Scan(typeof(Program).Assembly.GetTypes()), ConsoleHostIO.Instance)
        {
            Logger = loggerFactory.CreateLogger<Program>();

            Settings = Configuration.GetSection(typeof(ToolsSettings).Name)?.Get<ToolsSettings>();
            _queryDispatcher = new Lazy<IQueryDispatcher>(() => new ApiProxyQueryDispatcher(this));
            _commandDispatcher = new Lazy<ICommandDispatcher>(() => new ApiProxyCommandDispatcher(this));
        }

        Program(Program prototype) : base(prototype)
        {
            Settings = prototype.Settings;
            _queryDispatcher = prototype._queryDispatcher;
            _commandDispatcher = prototype._commandDispatcher;
        }

        public ToolsSettings Settings { get; }

        public IQueryDispatcher QueryDispatcher => _queryDispatcher.Value;

        public ICommandDispatcher CommandDispatcher => _commandDispatcher.Value;

        public override string AppName => Environment.ApplicationName;

        protected override IEnumerable<string> GetInstructions()
        {
            yield return $"You may provide API credentials by specifying /{ApiOperation.ApiUserNameOption}=<user-name> and /{ApiOperation.ApiPasswordOption}=<password> options.";
        }

        protected override string DefaultOperationName => SessionOperation.Name;

        public string ApiAuthToken { get; set; }
        public NetworkCredential ApiCredentials { get; set; }

        public bool IsNested { get; private set; }

        public int ExecuteNested(string[] args)
        {
            IsNested = true;
            try { return new Program(this).Execute(args); }
            finally { IsNested = false; }
        }
    }
}