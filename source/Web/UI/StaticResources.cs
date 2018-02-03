using Karambolo.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Web.Mvc;
using Karambolo.Common.Localization;

namespace AspNetSkeleton.UI
{
    public static class StaticResources
    {
        static ITextLocalizer T => DependencyResolver.Current.GetService<ITextLocalizer>();

        static TAttribute Localize<TAttribute>(this TAttribute @this, Expression<Func<string>> errorMessageExpression = null,
            Expression<Func<string>> fallbackErrorMessageExpression = null)
            where TAttribute : ValidationAttribute
        {
            var property =
                errorMessageExpression != null ? Lambda.Property(errorMessageExpression) :
                fallbackErrorMessageExpression != null ? Lambda.Property(fallbackErrorMessageExpression) :
                throw new ArgumentNullException(null, nameof(fallbackErrorMessageExpression));
                
            @this.ErrorMessageResourceType = property.DeclaringType;
            @this.ErrorMessageResourceName = property.Name;
            return @this;
        }

        #region Display
        public static DisplayAttribute Localize(this DisplayAttribute @this, Expression<Func<string>> nameExpression = null,
            Expression<Func<string>> shortNameExpression = null, Expression<Func<string>> descriptionExpression = null)
        {
            Type declaringType = null;
            string name = null;
            if (nameExpression != null)
            {
                var property = Lambda.Property(nameExpression);
                declaringType = property.DeclaringType;
                name = property.Name;
            }

            string shortName = null;
            if (shortNameExpression != null)
            {
                var property = Lambda.Property(shortNameExpression);
                if (declaringType != null && property.DeclaringType != declaringType)
                    throw new ArgumentException(null, nameof(shortNameExpression));

                declaringType = property.DeclaringType;
                shortName = property.Name;
            }

            string description = null;
            if (descriptionExpression != null)
            {
                var property = Lambda.Property(descriptionExpression);
                if (declaringType != null && property.DeclaringType != declaringType)
                    throw new ArgumentException(null, nameof(descriptionExpression));

                declaringType = property.DeclaringType;
                description = property.Name;
            }

            if (declaringType == null)
                throw new ArgumentNullException(nameof(nameExpression));

            @this.ResourceType = declaringType;
            @this.Name = name;
            @this.ShortName = shortName;
            @this.Description = description;

            return @this;
        }
        #endregion

        #region Required
        public static RequiredAttribute Localize(this RequiredAttribute @this, Expression<Func<string>> expression = null)
        {
            return @this.Localize(expression, () => RequiredErrorText);
        }

        public static string RequiredErrorText => T["The {0} field is required."];
        #endregion

        #region RegularExpressionAttribute
        public static RegularExpressionAttribute Localize(this RegularExpressionAttribute @this, Expression<Func<string>> expression = null)
        {
            return @this.Localize(expression, () => RegExErrorText);
        }

        public static string RegExErrorText => T["The field {0} must match the regular expression '{1}'."];
        #endregion

        #region MaxLengthAttribute
        public static MaxLengthAttribute Localize(this MaxLengthAttribute @this, Expression<Func<string>> expression = null)
        {
            return @this.Localize(expression, () => MaxLengthErrorText);
        }

        public static string MaxLengthErrorText => T["The field {0} must be a string or array type with a maximum length of '{1}'."];
        #endregion

        #region MaxLengthAttribute
        public static StringLengthAttribute Localize(this StringLengthAttribute @this, Expression<Func<string>> expression = null)
        {
            return @this.Localize(expression, () => StringLengthErrorText);
        }

        public static string StringLengthErrorText => T["The field {0} must be a string with a minimum length of {2} and a maximum length of {1}."];
        #endregion

        #region CompareAttribute
        public static System.ComponentModel.DataAnnotations.CompareAttribute Localize(this System.ComponentModel.DataAnnotations.CompareAttribute @this,
            Expression<Func<string>> expression = null)
        {
            return @this.Localize(expression, () => CompareErrorText);
        }

        public static string CompareErrorText => T["'{0}' and '{1}' do not match."];
        #endregion
    }
}