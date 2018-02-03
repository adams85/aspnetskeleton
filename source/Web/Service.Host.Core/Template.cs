using Autofac;
using AspNetSkeleton.Service.Host.Core.Infrastructure;
using RazorEngine.Templating;

namespace AspNetSkeleton.Service.Host.Core
{
    public class Template<T> : TemplateBase<T>
    {
        readonly IServiceHostCoreSettings _settings;

        public Template()
        {
            _settings = ServiceHostCoreModule.RootLifetimeScope.Resolve<IServiceHostCoreSettings>();
        }

        public IServiceHostCoreSettings Settings => _settings;
    }
}