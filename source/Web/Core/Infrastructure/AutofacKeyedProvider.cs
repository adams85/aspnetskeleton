using AspNetSkeleton.Common.Infrastructure;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using System;

namespace AspNetSkeleton.Core.Infrastructure
{
    public class AutofacKeyedProvider<T> : IKeyedProvider<T>
    {
        readonly IComponentContext _context;

        public AutofacKeyedProvider(IComponentContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _context = context;
        }

        public bool TryProvideFor(object key, out T instance)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var result = _context.TryResolveKeyed(key, typeof(T), out object obj);
            instance = (T)obj;
            return result;
        }

        public T ProvideFor(object key)
        {
            return 
                TryProvideFor(key, out T instance) ? 
                instance : 
                throw new ComponentNotRegisteredException(new KeyedService(key, typeof(T)));
        }
    }
}
