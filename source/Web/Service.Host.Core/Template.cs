using Autofac;
using RazorLight;
using AspNetSkeleton.Core.Infrastructure;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using AspNetSkeleton.Core;

namespace AspNetSkeleton.Service.Host.Core
{
    public abstract class Template<T> : TemplatePage<T>
    {
        public CoreSettings CoreSettings => AppScope.Current.LifetimeScope.Resolve<IOptions<CoreSettings>>().Value;

        public string FormatDateTime(DateTime value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:d MMM yyyy} {0:HH:mm} (GMT)", value);
        }
    }
}