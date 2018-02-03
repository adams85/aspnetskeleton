using System;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Runtime.ExceptionServices;
using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.DataAccess;
using AspNetSkeleton.DataAccess.MySql;

namespace AspNetSkeleton.DeployTools
{
    public abstract class DbOperation : Operation
    {
        public const string ServiceHostPathOption = "host-path";

#if DISTRIBUTED
        const string serviceHostAssemblyFileName = "AspNetSkeleton.Service.Host.exe";
        const string serviceHostConfigFileName = "app.config";
        const string serviceHostRelativeBinPath = @"..\..\..\Service.Host\bin";
#else
        const string serviceHostAssemblyFileName = "AspNetSkeleton.UI.dll";
        const string serviceHostConfigFileName = "web.config";
        const string serviceHostRelativeBinPath = @"..\..\..\UI\bin";
#endif

        static void RegisterDbProviders(string providerName)
        {
            var systemDataSection = ConfigurationManager.GetSection("system.data") as System.Data.DataSet;
            systemDataSection.Tables[0].Clear();
            if (providerName == MySqlDbConfiguration.ProviderName)
            {
                systemDataSection.Tables[0].Rows.Add("MySQL Data Provider", ".Net Framework Data Provider for MySQL", providerName, MySqlDbConfiguration.SqlClientFactoryType.AssemblyQualifiedName);
                DbConfiguration.SetConfiguration(new MySqlDbConfiguration());
            }
            else
                throw new NotSupportedException();
        }

        readonly ConnectionStringSettings _connString;

        protected DbOperation(string[] args, IOperationContext context) : base(args, context)
        {
            ServiceHostPath = GetServiceHostPath();
            if (ServiceHostPath != null)
            {
                var configFilePath = Path.Combine(ServiceHostPath, serviceHostConfigFileName);
                var configMap = new ExeConfigurationFileMap
                {
                    ExeConfigFilename = configFilePath
                };
                var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
                _connString = config.ConnectionStrings.ConnectionStrings[typeof(DataContext).Name];

                if (_connString == null)
                    throw new InvalidOperationException($"Connection string not found in {configFilePath}.");

                RegisterDbProviders(_connString.ProviderName);
            }
        }

        protected string ServiceHostPath { get; private set; }

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
                string serviceHostBinPath;
                var toolsAppPath = Program.AssemblyPath;
                if (File.Exists(Path.Combine(toolsAppPath, serviceHostAssemblyFileName)))
                    serviceHostBinPath = toolsAppPath;
                else if (!File.Exists(Path.Combine(serviceHostBinPath = Path.GetFullPath(Path.Combine(toolsAppPath, serviceHostRelativeBinPath)), serviceHostAssemblyFileName)))
                    serviceHostBinPath = null;

                serviceHostPath = Path.GetDirectoryName(serviceHostBinPath);
            }

            return serviceHostPath;
        }
        
        protected DbConnection CreateConnection()
        {
            var result = DbProviderFactories.GetFactory(_connString.ProviderName).CreateConnection();
            try 
	        {                
                result.ConnectionString = _connString.ConnectionString;
                return result;
	        }
	        catch (Exception ex)
	        {
	            result?.Dispose();

	            ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
	        }
        }

        protected DbConnectionInfo CreateConnectionInfo()
        {
            return new DbConnectionInfo(_connString.ConnectionString, _connString.ProviderName);
        }
    }
}
