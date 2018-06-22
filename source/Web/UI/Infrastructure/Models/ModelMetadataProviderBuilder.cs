using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Karambolo.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace AspNetSkeleton.UI.Infrastructure.Models
{
    public interface IModelMetadataBuilder<TBuilder> where TBuilder : IModelMetadataBuilder<TBuilder>
    {
        TBuilder BindingMetadata(Action<BindingMetadata> action);
        TBuilder DisplayMetadata(Action<DisplayMetadata> action);
        TBuilder ValidationMetadata(Action<ValidationMetadata> action);
    }

    public interface ITypeModelMetadataBuilder : IModelMetadataBuilder<ITypeModelMetadataBuilder>
    {
        ITypeModelMetadataBuilder InheritFrom(Type baseModelType);
        IPropertyModelMetadataBuilder Property(string propertyName);
    }

    public interface ITypeModelMetadataBuilder<TModel> : ITypeModelMetadataBuilder, IModelMetadataBuilder<ITypeModelMetadataBuilder<TModel>>
    {
        ITypeModelMetadataBuilder<TModel> InheritFrom<TBaseModel>();
        IPropertyModelMetadataBuilder Property<TProperty>(Expression<Func<TModel, TProperty>> expression);
    }

    public interface IPropertyModelMetadataBuilder : IModelMetadataBuilder<IPropertyModelMetadataBuilder> { }

    public interface IModelMetadataProviderBuilder
    {
        IMetadataDetailsProvider Build();
    }

    public interface IModelMetadataBuilder
    {
        ITypeModelMetadataBuilder Model(Type modelType);
        ITypeModelMetadataBuilder<TModel> Model<TModel>();
    }

    public class ModelMetadataProviderBuilder : IModelMetadataProviderBuilder, IModelMetadataBuilder
    {
        abstract class Builder<TBuilder> : IModelMetadataBuilder<TBuilder>
             where TBuilder : IModelMetadataBuilder<TBuilder>
        {
            public List<Action<BindingMetadata>> BindingMetadataSetters { get; } = new List<Action<BindingMetadata>>();
            public List<Action<DisplayMetadata>> DisplayMetadataSetters { get; } = new List<Action<DisplayMetadata>>();
            public List<Action<ValidationMetadata>> ValidationMetadataSetters { get; } = new List<Action<ValidationMetadata>>();

            protected abstract TBuilder This { get; }

            public TBuilder BindingMetadata(Action<BindingMetadata> action)
            {
                BindingMetadataSetters.Add(action);
                return This;
            }

            public TBuilder DisplayMetadata(Action<DisplayMetadata> action)
            {
                DisplayMetadataSetters.Add(action);
                return This;
            }

            public TBuilder ValidationMetadata(Action<ValidationMetadata> action)
            {
                ValidationMetadataSetters.Add(action);
                return This;
            }
        }

        class TypeBuilder : Builder<ITypeModelMetadataBuilder>, ITypeModelMetadataBuilder
        {
            protected override ITypeModelMetadataBuilder This => this;

            readonly Type _type;

            public TypeBuilder(Type type)
            {
                _type = type;
            }

            public List<Type> BaseTypes { get; } = new List<Type>();
            public Dictionary<PropertyInfo, PropertyBuilder> Properties { get; } = new Dictionary<PropertyInfo, PropertyBuilder>();

            public ITypeModelMetadataBuilder InheritFrom(Type baseModelType)
            {
                BaseTypes.Add(baseModelType);
                return this;
            }

            protected PropertyBuilder GetPropertyBuilder(PropertyInfo property)
            {
                if (!Properties.TryGetValue(property, out PropertyBuilder propertyBuilder))
                    Properties.Add(property, propertyBuilder = new PropertyBuilder());

                return propertyBuilder;
            }

            public IPropertyModelMetadataBuilder Property(string propertyName)
            {
                var property = _type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
                return GetPropertyBuilder(property);
            }
        }

        class TypeBuilder<TModel> : TypeBuilder, ITypeModelMetadataBuilder<TModel>
        {
            public TypeBuilder() : base(typeof(TModel)) { }

            public ITypeModelMetadataBuilder<TModel> InheritFrom<TBaseModel>()
            {
                InheritFrom(typeof(TBaseModel));
                return this;
            }

            public IPropertyModelMetadataBuilder Property<TProperty>(Expression<Func<TModel, TProperty>> expression)
            {
                var property = Lambda.Property(expression);
                return GetPropertyBuilder(property);
            }

            ITypeModelMetadataBuilder<TModel> IModelMetadataBuilder<ITypeModelMetadataBuilder<TModel>>.BindingMetadata(Action<BindingMetadata> action)
            {
                BindingMetadata(action);
                return this;
            }

            ITypeModelMetadataBuilder<TModel> IModelMetadataBuilder<ITypeModelMetadataBuilder<TModel>>.DisplayMetadata(Action<DisplayMetadata> action)
            {
                DisplayMetadata(action);
                return this;
            }

            ITypeModelMetadataBuilder<TModel> IModelMetadataBuilder<ITypeModelMetadataBuilder<TModel>>.ValidationMetadata(Action<ValidationMetadata> action)
            {
                ValidationMetadata(action);
                return this;
            }
        }

        class PropertyBuilder : Builder<IPropertyModelMetadataBuilder>, IPropertyModelMetadataBuilder
        {
            protected override IPropertyModelMetadataBuilder This => this;
        }

        class Provider : DynamicModelMetadataProvider
        {
            struct Configuration
            {
                public Action<BindingMetadata>[] BindingMetadataSetters;
                public Action<DisplayMetadata>[] DisplayMetadataSetters;
                public Action<ValidationMetadata>[] ValidationMetadataSetters;
            }

            readonly Dictionary<ModelMetadataIdentity, Configuration> _configurations = new Dictionary<ModelMetadataIdentity, Configuration>();

            public Provider(ModelMetadataProviderBuilder builder)
            {
                foreach (var kvp in builder._types)
                    AddType(kvp.Key, kvp.Value, builder);
            }

            Configuration AddType(Type type, TypeBuilder builder, ModelMetadataProviderBuilder providerBuilder)
            {
                var bindingMetadataSetters = new List<Action<BindingMetadata>>();
                var displayMetadataSetters = new List<Action<DisplayMetadata>>();
                var validationMetadataSetters = new List<Action<ValidationMetadata>>();

                Configuration configuration;
                var n = builder.BaseTypes.Count;
                for (var i = 0; i < n; i++)
                {
                    var baseType = builder.BaseTypes[i];
                    var baseBuilder = providerBuilder._types[baseType];

                    if (!_configurations.TryGetValue(ModelMetadataIdentity.ForType(baseType), out configuration))
                        configuration = AddType(baseType, baseBuilder, providerBuilder);

                    bindingMetadataSetters.AddRange(configuration.BindingMetadataSetters);
                    displayMetadataSetters.AddRange(configuration.DisplayMetadataSetters);
                    validationMetadataSetters.AddRange(configuration.ValidationMetadataSetters);

                    foreach (var kvp in baseBuilder.Properties)
                        if (type.GetProperty(kvp.Key.Name, BindingFlags.Instance | BindingFlags.Public) != null)
                            AddProperty(kvp.Key, kvp.Value, type);
                }

                bindingMetadataSetters.AddRange(builder.BindingMetadataSetters);
                displayMetadataSetters.AddRange(builder.DisplayMetadataSetters);
                validationMetadataSetters.AddRange(builder.ValidationMetadataSetters);

                configuration = new Configuration
                {
                    BindingMetadataSetters = bindingMetadataSetters.Count > 0 ? bindingMetadataSetters.ToArray() : ArrayUtils.Empty<Action<BindingMetadata>>(),
                    DisplayMetadataSetters = displayMetadataSetters.Count > 0 ? displayMetadataSetters.ToArray() : ArrayUtils.Empty<Action<DisplayMetadata>>(),
                    ValidationMetadataSetters = validationMetadataSetters.Count > 0 ? validationMetadataSetters.ToArray() : ArrayUtils.Empty<Action<ValidationMetadata>>(),
                };

                _configurations.Add(ModelMetadataIdentity.ForType(type), configuration);

                foreach (var kvp in builder.Properties)
                    AddProperty(kvp.Key, kvp.Value, type);

                return configuration;
            }

            void AddProperty(PropertyInfo property, PropertyBuilder builder, Type containerType)
            {
                var bindingMetadataSetters = new List<Action<BindingMetadata>>();
                var displayMetadataSetters = new List<Action<DisplayMetadata>>();
                var validationMetadataSetters = new List<Action<ValidationMetadata>>();

                bindingMetadataSetters.AddRange(builder.BindingMetadataSetters);
                displayMetadataSetters.AddRange(builder.DisplayMetadataSetters);
                validationMetadataSetters.AddRange(builder.ValidationMetadataSetters);

                var configuration = new Configuration
                {
                    BindingMetadataSetters = bindingMetadataSetters.Count > 0 ? bindingMetadataSetters.ToArray() : ArrayUtils.Empty<Action<BindingMetadata>>(),
                    DisplayMetadataSetters = displayMetadataSetters.Count > 0 ? displayMetadataSetters.ToArray() : ArrayUtils.Empty<Action<DisplayMetadata>>(),
                    ValidationMetadataSetters = validationMetadataSetters.Count > 0 ? validationMetadataSetters.ToArray() : ArrayUtils.Empty<Action<ValidationMetadata>>(),
                };

                _configurations.Add(ModelMetadataIdentity.ForProperty(property.PropertyType, property.Name, containerType), configuration);
            }

            protected override IList<Action<BindingMetadata>> GetBindingMetadataSetters(ModelMetadataIdentity key)
            {
                return _configurations.TryGetValue(key, out Configuration configuration) ? configuration.BindingMetadataSetters : ArrayUtils.Empty<Action<BindingMetadata>>();
            }

            protected override IList<Action<DisplayMetadata>> GetDisplayMetadataSetters(ModelMetadataIdentity key)
            {
                return _configurations.TryGetValue(key, out Configuration configuration) ? configuration.DisplayMetadataSetters : ArrayUtils.Empty<Action<DisplayMetadata>>();
            }

            protected override IList<Action<ValidationMetadata>> GetValidationMetadataSetters(ModelMetadataIdentity key)
            {
                return _configurations.TryGetValue(key, out Configuration configuration) ? configuration.ValidationMetadataSetters : ArrayUtils.Empty<Action<ValidationMetadata>>();
            }
        }

        Dictionary<Type, TypeBuilder> _types { get; } = new Dictionary<Type, TypeBuilder>();

        TBuilder GetTypeBuilder<TBuilder>(Type type, Func<Type, TBuilder> factory)
            where TBuilder : TypeBuilder
        {
            if (!_types.TryGetValue(type, out TypeBuilder typeBuilder))
                _types.Add(type, typeBuilder = factory(type));

            return (TBuilder)typeBuilder;
        }

        public ITypeModelMetadataBuilder Model(Type modelType)
        {
            return GetTypeBuilder(modelType, t => new TypeBuilder(t));
        }

        public ITypeModelMetadataBuilder<TModel> Model<TModel>()
        {
            return GetTypeBuilder(typeof(TModel), t => new TypeBuilder<TModel>());
        }

        public IMetadataDetailsProvider Build()
        {
            return new Provider(this);
        }
    }

    public static class ModelMetadataBuilderExtensions
    {
        public static IModelMetadataBuilder<TBuilder> DisplayName<TBuilder>(this IModelMetadataBuilder<TBuilder> @this, Func<string> valueFactory)
            where TBuilder : IModelMetadataBuilder<TBuilder>
        {
            @this.DisplayMetadata(md => md.DisplayName = valueFactory);
            return @this;
        }

        public static IModelMetadataBuilder<TBuilder> Validator<TBuilder>(this IModelMetadataBuilder<TBuilder> @this, ValidationAttribute validator)
            where TBuilder : IModelMetadataBuilder<TBuilder>
        {
            @this.ValidationMetadata(md => md.ValidatorMetadata.Add(validator));
            return @this;
        }

        public static IModelMetadataBuilder<TBuilder> BinderType<TBuilder>(this IModelMetadataBuilder<TBuilder> @this, Type binderType)
            where TBuilder : IModelMetadataBuilder<TBuilder>
        {
            @this.BindingMetadata(md => md.BinderType = binderType);
            return @this;
        }
    }
}
