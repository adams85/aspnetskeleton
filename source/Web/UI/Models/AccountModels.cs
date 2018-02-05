using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.Service.Contract.Queries;
using AspNetSkeleton.UI.Infrastructure.Models;
using Karambolo.Common;
using System.ComponentModel.DataAnnotations;
using Karambolo.Common.Localization;
using System.Web.Mvc;

namespace AspNetSkeleton.UI.Models
{
    [HandledAs(typeof(AuthenticateUserQuery))]
    public class LoginModel : AuthenticateUserQuery
    {
        public class Configurer : ModelAttributesProviderConfigurer
        {
            public static string UserNameDisplayName => T["E-mail address"];
            public static string PasswordDisplayName => T["Password"];
            public static string RememberMeDisplayName => T["Remember me?"];

            public override void Configure(ModelAttributesProviderBuilder builder)
            {
                builder.Model<LoginModel>().Property(m => m.UserName)
                    .Apply(new DisplayAttribute().Localize(() => UserNameDisplayName))
                    .Apply(new RequiredAttribute().Localize());

                builder.Model<LoginModel>().Property(m => m.Password)
                    .Apply(new DisplayAttribute().Localize(() => PasswordDisplayName))
                    .Apply(new RequiredAttribute().Localize())
                    .Apply(new DataTypeAttribute(DataType.Password));

                builder.Model<LoginModel>().Property(m => m.RememberMe)
                    .Apply(new DisplayAttribute().Localize(() => RememberMeDisplayName));
            }
        }

        public bool RememberMe { get; set; }
    }

    [HandledAs(typeof(CreateUserCommand))]
    public class RegisterModel : CreateUserCommand
    {
        public class Configurer : ModelAttributesProviderConfigurer
        {
            public static string UserNameDisplayName => T["E-mail address"];
            public static string PasswordDisplayName => T["Password"];
            public static string ConfirmPasswordDisplayName => T["Confirm password"];
            public static string FirstNameDisplayName => T["First name"];
            public static string LastNameDisplayName => T["Last name"];

            public static string InvalidEmailErrorText => T["Please enter a valid e-mail address."];
            public static string PasswordLengthErrorText => T["The {0} must be at least {2} characters long."];
            public static string ConfirmPasswordErrorText => T["The password and confirmation password do not match."];

            public override void Configure(ModelAttributesProviderBuilder builder)
            {
                builder.Model<RegisterModel>().Property(m => m.UserName)
                    .Apply(new DisplayAttribute().Localize(() => UserNameDisplayName))
                    .Apply(new RequiredAttribute().Localize())
                    .Apply(new RegularExpressionAttribute(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$")
                        .Localize(() => InvalidEmailErrorText))
                    .Apply(new MaxLengthAttribute(320).Localize());

                builder.Model<RegisterModel>().Property(m => m.Password)
                    .Apply(new DisplayAttribute().Localize(() => PasswordDisplayName))
                    .Apply(new RequiredAttribute().Localize())
                    .Apply(new DataTypeAttribute(DataType.Password))
                    .Apply(new StringLengthAttribute(100) { MinimumLength = 6 }.Localize(() => PasswordLengthErrorText));

                builder.Model<RegisterModel>().Property(m => m.ConfirmPassword)
                    .Apply(new DisplayAttribute().Localize(() => ConfirmPasswordDisplayName))
                    .Apply(new DataTypeAttribute(DataType.Password))
                    .Apply(new System.ComponentModel.DataAnnotations.CompareAttribute(Lambda.Property((RegisterModel m) => m.Password).Name).Localize(() => ConfirmPasswordErrorText));

                builder.Model<RegisterModel>().Property(m => m.FirstName)
                    .Apply(new DisplayAttribute().Localize(() => FirstNameDisplayName))
                    .Apply(new RequiredAttribute().Localize())
                    .Apply(new MaxLengthAttribute(100).Localize());

                builder.Model<RegisterModel>().Property(m => m.LastName)
                    .Apply(new DisplayAttribute().Localize(() => LastNameDisplayName))
                    .Apply(new RequiredAttribute().Localize())
                    .Apply(new MaxLengthAttribute(100).Localize());
            }
        }

        public string ConfirmPassword { get; set; }
    }

    [HandledAs(typeof(ResetPasswordCommand))]
    public class ResetPasswordModel : ResetPasswordCommand
    {
        public class Configurer : ModelAttributesProviderConfigurer
        {
            public static string UserNameDisplayName => T["E-mail address"];
            public static string InvalidEmailErrorText => T["Please enter a valid e-mail address."];

            public override void Configure(ModelAttributesProviderBuilder builder)
            {
                builder.Model<ResetPasswordModel>().Property(m => m.UserName)
                    .Apply(new DisplayAttribute().Localize(() => UserNameDisplayName))
                    .Apply(new RequiredAttribute().Localize())
                    .Apply(new RegularExpressionAttribute(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$")
                        .Localize(() => InvalidEmailErrorText))
                    .Apply(new MaxLengthAttribute(320).Localize());
            }
        }

        public bool? Success { get; set; }
    }

    [HandledAs(typeof(ChangePasswordCommand))]
    public class SetPasswordModel : ChangePasswordCommand
    {
        public class Configurer : ModelAttributesProviderConfigurer
        {
            public static string NewPasswordDisplayName => T["New password"];
            public static string ConfirmPasswordDisplayName => T["Confirm password"];

            public static string PasswordLengthErrorText => T["The {0} must be at least {2} characters long."];
            public static string ConfirmPasswordErrorText => T["The password and confirmation password do not match."];

            public override void Configure(ModelAttributesProviderBuilder builder)
            {
                builder.Model<SetPasswordModel>().Property(m => m.NewPassword)
                    .Apply(new DisplayAttribute().Localize(() => NewPasswordDisplayName))
                    .Apply(new RequiredAttribute().Localize())
                    .Apply(new DataTypeAttribute(DataType.Password))
                    .Apply(new StringLengthAttribute(100) { MinimumLength = 6 }.Localize(() => PasswordLengthErrorText));

                builder.Model<SetPasswordModel>().Property(m => m.ConfirmPassword)
                    .Apply(new DisplayAttribute().Localize(() => ConfirmPasswordDisplayName))
                    .Apply(new DataTypeAttribute(DataType.Password))
                    .Apply(new System.ComponentModel.DataAnnotations.CompareAttribute(Lambda.Property((SetPasswordModel m) => m.NewPassword).Name).Localize(() => ConfirmPasswordErrorText));
            }
        }

        public string ConfirmPassword { get; set; }
        public bool? Success { get; set; }
    }
}
