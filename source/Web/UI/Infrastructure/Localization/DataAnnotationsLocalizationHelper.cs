using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace AspNetSkeleton.UI.Infrastructure.Localization
{
    // Data annotations localization is (still) broken, default error messages values won't be put through the localizer.
    // https://github.com/aspnet/Localization/issues/286
    // https://github.com/dotnet/corefx/issues/25128
    // WORKAROUND: specifying explicit error messages until it's fixed...
    // Original messages: https://github.com/dotnet/corefx/blob/master/src/System.ComponentModel.Annotations/src/Resources/Strings.resx
    public static class DataAnnotationsLocalizationHelper
    {
        // this is only for supporting POTools so that it can extract data annotation texts
        static IStringLocalizer T { get; } = NullStringLocalizer.Instance;

        #region Required
        public static RequiredAttribute Localize(this RequiredAttribute @this)
        {
            @this.ErrorMessage = RequiredErrorText;
            return @this;
        }

        public static string RequiredErrorText => T["The {0} field is required."];
        #endregion

        #region RegularExpressionAttribute
        public static RegularExpressionAttribute Localize(this RegularExpressionAttribute @this)
        {
            @this.ErrorMessage = RegExErrorText;
            return @this;
        }

        public static string RegExErrorText => T["The field {0} must match the regular expression '{1}'."];
        #endregion

        #region EmailAddressAttribute
        public static EmailAddressAttribute Localize(this EmailAddressAttribute @this)
        {
            @this.ErrorMessage = EmailAddressErrorText;
            return @this;
        }

        public static string EmailAddressErrorText => T["The {0} field is not a valid e-mail address."];
        #endregion

        #region MaxLengthAttribute
        public static MaxLengthAttribute Localize(this MaxLengthAttribute @this)
        {
            @this.ErrorMessage = MaxLengthErrorText;
            return @this;
        }

        public static string MaxLengthErrorText => T["The field {0} must be a string or array type with a maximum length of '{1}'."];
        #endregion

        #region MaxLengthAttribute
        public static StringLengthAttribute Localize(this StringLengthAttribute @this)
        {
            @this.ErrorMessage = StringLengthErrorText;
            return @this;
        }

        public static string StringLengthErrorText => T["The field {0} must be a string with a maximum length of {1}."];
        #endregion

        #region CompareAttribute
        public static System.ComponentModel.DataAnnotations.CompareAttribute Localize(this CompareAttribute @this)
        {
            @this.ErrorMessage = CompareErrorText;
            return @this;
        }

        public static string CompareErrorText => T["'{0}' and '{1}' do not match."];
        #endregion
    }
}
