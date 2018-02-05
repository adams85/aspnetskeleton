using Karambolo.Common;
using Karambolo.Common.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace AspNetSkeleton.UI.Infrastructure.Models
{
    public class FluentModelAttributesProvider : DynamicModelAttributesProvider
    {
        readonly IReadOnlyDictionary<Type, DynamicModelMetadata> _configuration;

        public FluentModelAttributesProvider(IReadOnlyDictionary<Type, DynamicModelMetadata> configuration)
        {
            _configuration = configuration;
        }

        protected override bool TryGetModelMetadata(Type containerType, out DynamicModelMetadata value)
        {
            return _configuration.TryGetValue(containerType, out value);
        }
    }

    public interface IModelPropertyAttributesConfigurer
    {
        IModelPropertyAttributesConfigurer Apply(Attribute attribute);
    }

    public interface IModelAttributesConfigurer<TModel>
    {
        IModelAttributesConfigurer<TModel> InheritFrom<TBaseModel>();
        IModelPropertyAttributesConfigurer Property<TProperty>(Expression<Func<TModel, TProperty>> expression);
    }

    public interface IModelAttributesProviderBuilder
    {
        IModelAttributesConfigurer<TModel> Model<TModel>();
        IDynamicModelAttributesProvider Build();
    }

    public interface IModelAttributesProviderConfigurer
    {
        void Configure(ModelAttributesProviderBuilder builder);
    }

    public abstract class ModelAttributesProviderConfigurer : IModelAttributesProviderConfigurer
    {
        protected static ITextLocalizer T => DependencyResolver.Current.GetService<ITextLocalizer>();

        public abstract void Configure(ModelAttributesProviderBuilder builder);
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
            List<Type> BaseTypes { get; }
            Dictionary<string, ModelPropertyConfiguration> PropertyConfigs { get; }
        }

        class ModelConfigurer<TModel> : IModelConfiguration, IModelAttributesConfigurer<TModel>
        {
            public List<Type> BaseTypes { get; } = new List<Type>();
            public Dictionary<string, ModelPropertyConfiguration> PropertyConfigs { get; } = new Dictionary<string, ModelPropertyConfiguration>();

            readonly ModelAttributesProviderBuilder _owner;

            public ModelConfigurer(ModelAttributesProviderBuilder owner)
            {
                _owner = owner;
            }

            public IModelAttributesConfigurer<TModel> InheritFrom<TBaseModel>()
            {
                BaseTypes.Add(typeof(TBaseModel));
                return this;
            }

            public IModelPropertyAttributesConfigurer Property<TProperty>(Expression<Func<TModel, TProperty>> expression)
            {
                var property = Lambda.Property(expression);
                if (!PropertyConfigs.TryGetValue(property.Name, out ModelPropertyConfiguration propertyConfig))
                    PropertyConfigs.Add(property.Name, propertyConfig = new ModelPropertyConfiguration());

                return propertyConfig;
            }
        }

        readonly Dictionary<Type, IModelConfiguration> _modelConfigs = new Dictionary<Type, IModelConfiguration>();

        public IModelAttributesConfigurer<TModel> Model<TModel>()
        {
            var type = typeof(TModel);
            if (!_modelConfigs.TryGetValue(type, out IModelConfiguration config))
                _modelConfigs.Add(type, config = new ModelConfigurer<TModel>(this));

            return (ModelConfigurer<TModel>)config;
        }

        static IEnumerable<Attribute> MergeAttributes(IEnumerable<Attribute> attributes, IList<Attribute> otherAttributes)
        {
            var n = otherAttributes.Count;
            if (n == 0)
                return attributes;

            var attributesByType = attributes.ToDictionary(a => a.GetType(), Identity<Attribute>.Func);

            Type otherAttributeType;
            Attribute otherAttribute;
            for (var i = 0; i < n; i++)
                if (!attributesByType.ContainsKey(otherAttributeType = (otherAttribute = otherAttributes[i]).GetType()))
                    attributesByType.Add(otherAttributeType, otherAttribute);

            return attributesByType.Values;
        }

        public IDynamicModelAttributesProvider Build()
        {
            var configuration = _modelConfigs
                .ToDictionary(
                    mc => mc.Key,
                    mc => new DynamicModelMetadata(mc.Value.BaseTypes.ToArray(),
                        mc.Value.PropertyConfigs.ToDictionary(pc => pc.Key, pc => pc.Value.Attributes.ToArray())));

            return new FluentModelAttributesProvider(configuration);
        }
    }
}