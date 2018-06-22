using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AspNetSkeleton.Base.Utils
{
    public static class ConfigurationUtils
    {
        public static IConfigurationSection GetSectionFor<TOptions>(this IConfigurationRoot configuration) where TOptions : class
        {
            return configuration.GetSection(typeof(TOptions).Name);
        }

        public static TOptions GetByConvention<TOptions>(this IConfigurationRoot configuration) where TOptions : class
        {            
            return configuration.GetSectionFor<TOptions>().Get<TOptions>();
        }

        public static IServiceCollection ConfigureByConvention<TOptions>(this IServiceCollection @this, IConfigurationRoot configuration)
            where TOptions : class
        {
            return @this.Configure<TOptions>(configuration.GetSectionFor<TOptions>());
        }

        public static IServiceCollection Configure<TOptions, TDep>(this IServiceCollection @this, Action<TOptions, TDep> configure)
            where TOptions : class
            where TDep : class
        {
            return @this.AddSingleton<IConfigureOptions<TOptions>>(sp => new ConfigureNamedOptions<TOptions, TDep>(Options.DefaultName, sp.GetRequiredService<TDep>(), configure));
        }
    }
}
