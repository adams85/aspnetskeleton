using System;
using System.Linq;
using Autofac;
using Karambolo.Common;
using System.Reflection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using AspNetSkeleton.Common.Infrastructure;
using Microsoft.Extensions.Localization;

namespace AspNetSkeleton.Core.Infrastructure
{
    public interface IPropertyInjectorFactory
    {
        Action<object, IComponentContext> Create(Type type);
    }

    public class LoggerPropertyInjectorFactory : IPropertyInjectorFactory
    {
        readonly ConcurrentDictionary<Type, Action<object, IComponentContext>> _injectors = new ConcurrentDictionary<Type, Action<object, IComponentContext>>();

        public Action<object, IComponentContext> Create(Type type)
        {
            return _injectors.GetOrAdd(type, t =>
            {
                var property = t.GetProperty("Logger", BindingFlags.Instance | BindingFlags.Public, null, typeof(ILogger), Type.EmptyTypes, null);
                if (property == null)
                    return null;

                var loggerOptionsAttribute = property.GetAttributes<LoggerOptionsAttribute>(true).SingleOrDefault();
                var loggerSource = loggerOptionsAttribute?.SourceName ?? t.FullName;

                Action<object, IComponentContext> injector = (inst, ctx) =>
                {
                    var logger = ctx.Resolve<ILoggerFactory>().CreateLogger(loggerSource);
                    property.SetValue(inst, logger, null);
                };

                return injector;
            });
        }
    }

    public class TextLocalizerInjectorFactory : IPropertyInjectorFactory
    {
        readonly ConcurrentDictionary<Type, Action<object, IComponentContext>> _injectors = new ConcurrentDictionary<Type, Action<object, IComponentContext>>();

        public Action<object, IComponentContext> Create(Type type)
        {
            return _injectors.GetOrAdd(type, t =>
            {
                var property = t.GetProperty("T", BindingFlags.Instance | BindingFlags.Public, null, typeof(IStringLocalizer), Type.EmptyTypes, null);
                if (property == null)
                    return null;

                Action<object, IComponentContext> injector = (inst, ctx) =>
                {
                    var localizerFactory = ctx.Resolve<IStringLocalizerFactory>();
                    property.SetValue(inst, localizerFactory.Create(t), null);
                };

                return injector;
            });
        }
    }
}
