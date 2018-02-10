using System;
using System.Collections.Generic;
using System.IO;
using AspNetSkeleton.Base;
using AspNetSkeleton.Base.Utils;
using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.DataAccess;
using AspNetSkeleton.DataAccess.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace AspNetSkeleton.DeployTools
{
    public abstract class DbOperation : Operation
    {
        class DbServicesFactory
        {
            public Func<DbContext, IOperationContext, IDbManager> DbManagerFactory { get; set; }
            public Func<string, IDbMigrationProvider> DbMigrationProviderFactory { get; set; }
        }

        public const string ServiceHostPathOption = "host-path";

        static readonly string migrationScriptsBasePath = Path.Combine(AppEnvironment.Instance.AppBasePath, "Migrations");

        static readonly Dictionary<string, DbServicesFactory> dbServicesRegistry = new Dictionary<string, DbServicesFactory>
        {
            {
                DataAccessContants.MySqlProviderName, new DbServicesFactory
                {
                    DbManagerFactory = (dc, ctx) => new MySqlDbManager(dc, ctx.As<IDbOperationContext>().Clock),
                    DbMigrationProviderFactory = bp => new ScriptFileMigrationProvider(bp),
                }
            },
        };

#if DISTRIBUTED
        const string serviceHostAssemblyFileName = "Web.Service.Host.dll";
        const string serviceHostConfigFileName = "appsettings.json";
        const string serviceHostRelativeBinPath = @"..\..\..\Service.Host\bin";
#else
        const string serviceHostAssemblyFileName = "Web.UI.dll";
        const string serviceHostConfigFileName = "appsettings.Monolithic.json";
        const string serviceHostRelativeBinPath = @"..\..\..\UI\bin";
#endif

        readonly IDbConfigurationProvider _dbConfigurationProvider;

        protected DbOperation(string[] args, IOperationContext context) : base(args, context)
        {
            ServiceHostPath = GetServiceHostPath();
            if (ServiceHostPath != null)
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(ServiceHostPath)
                    .AddJsonFile(serviceHostConfigFileName)
                    .Build();

                var dbConfiguration = config.GetByConvention<DbConfiguration>();

                var dbConfigurationProvider = new DbConfigurationProvider(new OptionsWrapper<DbConfiguration>(dbConfiguration));
                dbConfigurationProvider.Initialize();
                _dbConfigurationProvider = dbConfigurationProvider;
            }
        }

        protected string ServiceHostPath { get; }

        protected DataContext CreateDataContext()
        {
            return new DataContext(_dbConfigurationProvider);
        }

        protected IDbManager CreateDbManager(DbContext dbContext)
        {
            return dbServicesRegistry[dbContext.ProviderName].DbManagerFactory(dbContext, Context);
        }

        protected IDbMigrationProvider CreateDbMigrationProvider(DbContext dbContext)
        {
            return dbServicesRegistry[dbContext.ProviderName].DbMigrationProviderFactory(migrationScriptsBasePath);
        }

        protected abstract void ExecuteCore();

        public sealed override void Execute()
        {
            if (ServiceHostPath == null)
                throw new OperationErrorException("Host application not found.");

            ExecuteCore();
        }

        string GetServiceHostPath()
        {
            if (!OptionalArgs.TryGetValue(ServiceHostPathOption, out string serviceHostPath))
            {
                var toolsAppPath = AppEnvironment.Instance.AppBasePath;
                if (File.Exists(Path.Combine(toolsAppPath, serviceHostAssemblyFileName)))
                    serviceHostPath = toolsAppPath;
                else if (!File.Exists(Path.Combine(serviceHostPath = Path.GetFullPath(Path.Combine(toolsAppPath, serviceHostRelativeBinPath)), serviceHostAssemblyFileName)))
                    serviceHostPath = null;
            }

            return serviceHostPath;
        }
    }
}
