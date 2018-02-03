using System;
using System.Collections.Generic;
using System.Threading;
using AspNetSkeleton.Common.Infrastructure;
using LinqToDB.Configuration;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using Microsoft.Extensions.Options;

namespace AspNetSkeleton.DataAccess
{
    public interface IDbConfigurationProvider : IInitializer
    {
        string ProvideFor<TContext>() where TContext : IDbContext;
    }

    public class DbContextConfiguration
    {
        public string ProviderName { get; set; }
        public string ConnectionString { get; set; }
    }

    public class DbProviderConfiguration
    {
        public string FactoryType { get; set; }
        public NamedValue[] Attributes { get; set; }
    }

    public class DbConfiguration
    {
        public static string GetContextName<TContext>()
            where TContext : IDbContext
        {
            return typeof(TContext).Name;
        }

        public static string GetConfigurationString(string providerName, string dataContextName)
        {
            if (dataContextName == null)
                throw new ArgumentNullException(nameof(dataContextName));

            if (providerName == null)
                throw new ArgumentNullException(nameof(providerName));

            return string.Concat(providerName, ".", dataContextName);
        }

        public static string GetProviderName(string configurationString)
        {
            if (configurationString == null)
                throw new ArgumentNullException(nameof(configurationString));

            return configurationString.Substring(0, configurationString.IndexOf('.'));
        }

        public Dictionary<string, DbContextConfiguration> Contexts { get; set; }
        public Dictionary<string, DbProviderConfiguration> Providers { get; set; }
        public string DefaultProvider { get; set; }
    }

    public class DbConfigurationProvider : IDbConfigurationProvider
    {
        readonly DbConfiguration _dbConfig;
        int _hasConfiguredFlag;

        public DbConfigurationProvider(IOptions<DbConfiguration> dbConfig)
        {
            if (dbConfig == null)
                throw new ArgumentNullException(nameof(dbConfig));

            if (dbConfig.Value == null)
                throw new ArgumentException("Value cannot be null.", nameof(dbConfig));

            _dbConfig = dbConfig.Value;
        }

        public void Initialize()
        {
            if (Interlocked.CompareExchange(ref _hasConfiguredFlag, 1, 0) != 0)
                throw new InvalidOperationException("Data access is already configured.");

            if (_dbConfig.Providers?.Count > 0)
            {
                foreach (var providerConfig in _dbConfig.Providers)
                {
                    var providerFactoryType = Type.GetType(providerConfig.Value.FactoryType, throwOnError: true);
                    var providerFactory = (IDataProviderFactory)Activator.CreateInstance(providerFactoryType);
                    DataConnection.AddDataProvider(providerConfig.Key, providerFactory.GetDataProvider(providerConfig.Value.Attributes));
                }

                if (_dbConfig.DefaultProvider == null || !_dbConfig.Providers.ContainsKey(_dbConfig.DefaultProvider))
                    throw new InvalidOperationException("Default provider is not specified.");

                DataConnection.DefaultDataProvider = _dbConfig.DefaultProvider;
            }

            if (_dbConfig.Contexts?.Count > 0)
            {
                if (DataConnection.DefaultDataProvider == null)
                    throw new InvalidOperationException("Default provider is not specified.");

                foreach (var contextConfig in _dbConfig.Contexts)
                    DataConnection.AddConfiguration(DbConfiguration.GetConfigurationString(contextConfig.Value.ProviderName, contextConfig.Key), contextConfig.Value.ConnectionString);
            }
        }

        public string ProvideFor<TContext>() where TContext : IDbContext
        {
            var dataContextName = DbConfiguration.GetContextName<TContext>();
            return DbConfiguration.GetConfigurationString(_dbConfig.Contexts[dataContextName].ProviderName, dataContextName);
        }
    }
}