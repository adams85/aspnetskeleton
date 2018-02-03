using AspNetSkeleton.AdminTools.Infrastructure;
using AspNetSkeleton.Api.Contract;
using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.Service.Contract;
using Karambolo.Common.Logging;
using System;
using System.Collections.Generic;
using System.Net;

namespace AspNetSkeleton.AdminTools
{
    class Program : OperationHost, IApiOperationContext
    {
        public static readonly string AssemblyName = typeof(Program).Assembly.GetName().Name;

        static int Main(string[] args)
        {
            return new Program().Execute(args);
        }

        readonly Lazy<IApiService> _apiService;
        readonly Lazy<IQueryDispatcher> _queryDispatcher;
        readonly Lazy<ICommandDispatcher> _commandDispatcher;

        public Program() 
            : base(OperationDescriptor.Scan(typeof(Program).Assembly.GetTypes()), ConsoleHostIO.Instance)
        {
            Logger = new TraceSourceLogger(AppName);

            Settings = new ToolsSettings();
            _apiService = new Lazy<IApiService>(() => new ApiService(Settings.ApiUrl));
            _queryDispatcher = new Lazy<IQueryDispatcher>(() => new ApiProxyQueryDispatcher(_apiService.Value, this));
            _commandDispatcher = new Lazy<ICommandDispatcher>(() => new ApiProxyCommandDispatcher(_apiService.Value, this));
        }

        Program(Program prototype) : base(prototype)
        {
            Settings = prototype.Settings;
            _apiService = prototype._apiService;
            _queryDispatcher = prototype._queryDispatcher;
            _commandDispatcher = prototype._commandDispatcher;
        }

        public IToolsSettings Settings { get; }

        public IApiService ApiService => _apiService.Value;

        public IQueryDispatcher QueryDispatcher => _queryDispatcher.Value;

        public ICommandDispatcher CommandDispatcher => _commandDispatcher.Value;

        public override string AppName => AssemblyName;

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