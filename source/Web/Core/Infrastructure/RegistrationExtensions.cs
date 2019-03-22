using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Activators.Delegate;
using Autofac;
using Autofac.Core.Activators.Reflection;
using Karambolo.Common;
using Autofac.Features.Scanning;

namespace AspNetSkeleton.Core.Infrastructure
{
    public interface IGenericDecoratorRegistrationBuilder : IHideObjectMembers
    {
        IGenericDecoratorRegistrationBuilder With(Type decoratorType, Func<IComponentContext, Type, bool> filter, Func<Type, IEnumerable<Parameter>> paramsGetter = null);
    }

    public static class RegistrationExtensions
    {
        class GenericDecoratorRegistration
        {
            public Type Type;
            public Func<IComponentContext, Type, bool> Filter;
            public Func<Type, IEnumerable<Parameter>> ParamsGetter;
        }

        class GenericDecoratorRegistrationBuilder : IGenericDecoratorRegistrationBuilder
        {
            readonly List<GenericDecoratorRegistration> _decorators = new List<GenericDecoratorRegistration>();

            public IEnumerable<GenericDecoratorRegistration> Decorators => _decorators;

            public IGenericDecoratorRegistrationBuilder With(Type decoratorType, Func<IComponentContext, Type, bool> filter, Func<Type, IEnumerable<Parameter>> paramsGetter)
            {
                if (decoratorType == null)
                    throw new ArgumentNullException(nameof(decoratorType));

                if (!decoratorType.IsGenericTypeDefinition)
                    throw new ArgumentException(null, nameof(decoratorType));

                var decorator = new GenericDecoratorRegistration
                {
                    Type = decoratorType,
                    Filter = filter,
                    ParamsGetter = paramsGetter
                };

                _decorators.Add(decorator);

                return this;
            }
        }

        class GenericDecoratorRegistrationSource : IRegistrationSource
        {
            readonly Type _decoratedType;
            readonly IEnumerable<GenericDecoratorRegistration> _decorators;
            readonly object _fromKey;
            readonly object _toKey;

            public GenericDecoratorRegistrationSource(Type decoratedType, IEnumerable<GenericDecoratorRegistration> decorators, object fromKey, object toKey)
            {
                _decoratedType = decoratedType;
                _decorators = decorators;
                _fromKey = fromKey;
                _toKey = toKey;
            }

            public bool IsAdapterForIndividualComponents => true;

            public IEnumerable<IComponentRegistration> RegistrationsFor(Autofac.Core.Service service, Func<Autofac.Core.Service, IEnumerable<IComponentRegistration>> registrationAccessor)
            {
                var swt = service as IServiceWithType;
                KeyedService ks;
                if (swt == null ||
                    (ks = new KeyedService(_fromKey, swt.ServiceType)) == service ||
                    !swt.ServiceType.IsGenericType || swt.ServiceType.GetGenericTypeDefinition() != _decoratedType)
                    return Enumerable.Empty<IComponentRegistration>();

                return registrationAccessor(ks).Select(cr => new ComponentRegistration(
                        Guid.NewGuid(),
                        BuildActivator(cr, swt),
                        cr.Lifetime,
                        cr.Sharing,
                        cr.Ownership,
                        new[] { _toKey != null ? (Autofac.Core.Service)new KeyedService(_toKey, swt.ServiceType) : new TypedService(swt.ServiceType) },
                        cr.Metadata,
                        cr));
            }

            DelegateActivator BuildActivator(IComponentRegistration cr, IServiceWithType swt)
            {
                var limitType = cr.Activator.LimitType;
                var decoratorsWithParams = _decorators
                    .Select(d => new { Value = d, Parameters = d.ParamsGetter?.Invoke(limitType) ?? Enumerable.Empty<Parameter>() })
                    .ToArray();

                return new DelegateActivator(cr.Activator.LimitType, (ctx, p) =>
                {
                    var typeArgs = swt.ServiceType.GetGenericArguments();
                    var service = ctx.ResolveKeyed(_fromKey, swt.ServiceType);

                    foreach (var decorator in decoratorsWithParams.Where(d => d.Value.Filter?.Invoke(ctx, limitType) ?? true))
                    {
                        var decoratorType = decorator.Value.Type.MakeGenericType(typeArgs);
                        var @params = decorator.Parameters.Prepend(new TypedParameter(swt.ServiceType, service));
                        var activator = new ReflectionActivator(decoratorType, new DefaultConstructorFinder(), new MostParametersConstructorSelector(),
                            @params, Enumerable.Empty<Parameter>());
                        service = activator.ActivateInstance(ctx, @params);
                    }

                    return service;
                });
            }
        }

        public static IGenericDecoratorRegistrationBuilder RegisterGenericDecorators(this ContainerBuilder builder, Type decoratedServiceType, object fromKey, object toKey = null)
        {
            if (decoratedServiceType == null)
                throw new ArgumentNullException(nameof(decoratedServiceType));

            if (fromKey == null)
                throw new ArgumentNullException(nameof(fromKey));

            if (!decoratedServiceType.IsGenericTypeDefinition)
                throw new ArgumentException(null, nameof(decoratedServiceType));

            var rb = new GenericDecoratorRegistrationBuilder();
            builder.RegisterCallback(cr => cr.AddRegistrationSource(new GenericDecoratorRegistrationSource(decoratedServiceType, rb.Decorators, fromKey, toKey)));

            return rb;
        }

        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> KeyedClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, object serviceKey, Type openGenericServiceType)
            where TScanningActivatorData : ScanningActivatorData
        {
            return registration.KeyedClosedTypesOf(t => serviceKey, openGenericServiceType);
        }

        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> KeyedClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Func<Type, object> serviceKeyProvider, Type openGenericServiceType)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (openGenericServiceType == null)
                throw new ArgumentNullException(nameof(openGenericServiceType));

            return registration
                .Where(t => t.IsClosedTypeOf(openGenericServiceType))
                .As(t => t.GetClosedInterfaces(openGenericServiceType).Select(t2 => new KeyedService(serviceKeyProvider(t), t2)));
        }
    }
}