using Karambolo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AspNetSkeleton.UI.Infrastructure.Models
{
    public class FluentModelAttributesProvider : IModelAttributesProvider
    {
        readonly IReadOnlyDictionary<Type, IReadOnlyDictionary<string, Attribute[]>> _configuration;

        public FluentModelAttributesProvider(IReadOnlyDictionary<Type, IReadOnlyDictionary<string, Attribute[]>> configuration)
        {
            _configuration = configuration;
        }

        public Attribute[] GetAttributes(Type containerType, string propertyName)
        {
            return
                _configuration.TryGetValue(containerType, out IReadOnlyDictionary<string, Attribute[]> propertyAttributes) &&
                propertyAttributes.TryGetValue(propertyName, out Attribute[] result) ?
                result :
                ArrayUtils.Empty<Attribute>();
        }
    }

    public interface IModelPropertyAttributesConfigurer
    {
        IModelPropertyAttributesConfigurer Apply(Attribute attribute);
    }

    public interface IModelAttributesConfigurer<TModel>
    {
        IModelPropertyAttributesConfigurer Property<TProperty>(Expression<Func<TModel, TProperty>> expression);
    }

    public interface IModelAttributesProviderBuilder
    {
        IModelAttributesConfigurer<TModel> Model<TModel>();
        IModelAttributesProvider Build();
    }

    public interface IModelAttributesProviderConfigurer
    {
        void Configure(ModelAttributesProviderBuilder builder);
    }

    public class ModelAttributesProviderBuilder : IModelAttributesProviderBuilder
    {
        class ModelPropertyConfiguration : IModelPropertyAttributesConfigurer
        {
            public List<Attribute> Attributes { get; } = new List<Attribute>();

            public IModelPropertyAttributesConfigurer Apply(Attribute attribute)
            {
                if (attribute == null)
                    throw new ArgumentNullException(nameof(attribute));

                Attributes.Add(attribute);

                return this;
            }
        }

        interface IModelConfiguration
        {
            Dictionary<PropertyInfo, ModelPropertyConfiguration> Configs { get; }
        }

        class ModelConfigurer<TModel> : IModelConfiguration, IModelAttributesConfigurer<TModel>
        {
            public Dictionary<PropertyInfo, ModelPropertyConfiguration> Configs { get; } = new Dictionary<PropertyInfo, ModelPropertyConfiguration>();

            public IModelPropertyAttributesConfigurer Property<TProperty>(Expression<Func<TModel, TProperty>> expression)
            {
                var property = Lambda.Property(expression);
                if (!Configs.TryGetValue(property, out ModelPropertyConfiguration config))
                    Configs.Add(property, config = new ModelPropertyConfiguration());

                return config;
            }
        }

        readonly Dictionary<Type, IModelConfiguration> _configs = new Dictionary<Type, IModelConfiguration>();

        public IModelAttributesConfigurer<TModel> Model<TModel>()
        {
            var type = typeof(TModel);
            if (!_configs.TryGetValue(type, out IModelConfiguration config))
                _configs.Add(type, config = new ModelConfigurer<TModel>());

            return (ModelConfigurer<TModel>)config;
        }

        public IModelAttributesProvider Build()
        {
            var configuration = _configs
                .ToDictionary(
                    mc => mc.Key,
                    mc => (IReadOnlyDictionary<string, Attribute[]>)mc.Value.Configs.ToDictionary(pc => pc.Key.Name, pc => pc.Value.Attributes.ToArray()));

            return new FluentModelAttributesProvider(configuration);
        }
    }
}