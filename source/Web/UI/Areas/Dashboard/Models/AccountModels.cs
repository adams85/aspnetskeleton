using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.UI.Infrastructure.Models;
using Karambolo.Common;
using System.ComponentModel.DataAnnotations;
using Karambolo.Common.Localization;
using System.Web.Mvc;

namespace AspNetSkeleton.UI.Areas.Dashboard.Models
{
    [HandledAs(typeof(ChangePasswordCommand))]
    public class ChangePasswordModel : ChangePasswordCommand
    {
        public class Configurer : IModelAttributesProviderConfigurer
        {
            static ITextLocalizer T => DependencyResolver.Current.GetService<ITextLocalizer>();

            public static string CurrentPasswordDisplayName => T["Current password"];
            public static string NewPasswordDisplayName => T["New password"];
            public static string ConfirmPasswordDisplayName => T["Confirm password"];

            public static string PasswordLengthErrorText => T["The {0} must be at least {2} characters long."];
            public static string ConfirmPasswordErrorText => T["The password and confirmation password do not match."];

            public void Configure(ModelAttributesProviderBuilder builder)
            {
                builder.Model<ChangePasswordModel>().Property(m => m.CurrentPassword)
                    .Apply(new DisplayAttribute().Localize(() => CurrentPasswordDisplayName))
                    .Apply(new RequiredAttribute().Localize())
                    .Apply(new DataTypeAttribute(DataType.Password));

                builder.Model<ChangePasswordModel>().Property(m => m.NewPassword)
                    .Apply(new DisplayAttribute().Localize(() => NewPasswordDisplayName))
                    .Apply(new RequiredAttribute().Localize())
                    .Apply(new DataTypeAttribute(DataType.Password))
                    .Apply(new StringLengthAttribute(100) { MinimumLength = 6 }.Localize(() => PasswordLengthErrorText));

                builder.Model<ChangePasswordModel>().Property(m => m.ConfirmPassword)
                    .Apply(new DisplayAttribute().Localize(() => ConfirmPasswordDisplayName))
                    .Apply(new DataTypeAttribute(DataType.Password))
                    .Apply(new System.ComponentModel.DataAnnotations.CompareAttribute(Lambda.Property((ChangePasswordModel m) => m.NewPassword).Name).Localize(() => ConfirmPasswordErrorText));
            }
        }

        public string CurrentPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public bool? Success { get; set; }
    }
}