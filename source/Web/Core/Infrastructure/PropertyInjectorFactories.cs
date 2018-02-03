using System;
using System.Linq;
using Autofac;
using Karambolo.Common;
using System.Reflection;
using Karambolo.Common.Logging;
using Karambolo.Common.Localization;
using System.Collections.Concurrent;
using AspNetSkeleton.Common.Infrastructure;

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
                var loggerSource = loggerOptionsAttribute?.SourceName ?? t.Assembly.GetName().Name;

                Action<object, IComponentContext> injector = (inst, ctx) =>
                {
                    var logger = ctx.Resolve<Func<string, ILogger>>()(loggerSource);
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
                var property = t.GetProperty("T", BindingFlags.Instance | BindingFlags.Public, null, typeof(ITextLocalizer), Type.EmptyTypes, null);
                if (property == null)
                    return null;

                Action<object, IComponentContext> injector = (inst, ctx) =>
                {
                    var localizer = ctx.Resolve<ITextLocalizer>();
                    property.SetValue(inst, localizer, null);
                };

                return injector;
            });
        }
    }
}
