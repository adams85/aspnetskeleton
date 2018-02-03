using Autofac;
using Autofac.Core;
using System;
using System.Diagnostics;
using System.Linq;

namespace AspNetSkeleton.Core.Infrastructure
{
    public class PropertyInjectorModule : Module
    {
        class ActivationHandler
        {
            readonly Action<object, IComponentContext>[] _injectors;

            public ActivationHandler(Action<object, IComponentContext>[] injectors)
            {
                _injectors = injectors;
            }

            [DebuggerStepThrough]
            public void Handle(object sender, ActivatedEventArgs<object> e)
            {
                var n = _injectors.Length;
                for (var i = 0; i < n; i++)
                    _injectors[i](e.Instance, e.Context);
            }
        }

        readonly IPropertyInjectorFactory[] _injectorFactories;

        public PropertyInjectorModule()
        {
            _injectorFactories = new IPropertyInjectorFactory[]
            {
                CreateLoggerPropertyInjectorFactory(),
                CreateTextLocalizerInjectorFactory(),
            };
        }

        protected virtual IPropertyInjectorFactory CreateLoggerPropertyInjectorFactory()
        {
            return new LoggerPropertyInjectorFactory();
        }

        protected virtual IPropertyInjectorFactory CreateTextLocalizerInjectorFactory()
        {
            return new TextLocalizerInjectorFactory();
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
        {
            // injecting properties
            var type = registration.Activator.LimitType;
            var injectors = _injectorFactories.Select(f => f.Create(type)).Where(f => f != null).ToArray();

            if (injectors.Length > 0)
                registration.Activated += new ActivationHandler(injectors).Handle;
        }
    }
}
