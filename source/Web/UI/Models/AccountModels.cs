using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.Service.Contract.Queries;
using AspNetSkeleton.UI.Infrastructure.Models;
using Karambolo.Common;
using System.ComponentModel.DataAnnotations;
using Karambolo.Common.Localization;
using AspNetSkeleton.UI.Infrastructure.Localization;

namespace AspNetSkeleton.UI.Models
{
    [HandledAs(typeof(AuthenticateUserQuery))]
    public class LoginModel : AuthenticateUserQuery
    {
        public class Configurer : ModelAttributesProviderConfigurer
        {
            public override void Configure(ModelAttributesProviderBuilder builder)
            {
                builder.Model<LoginModel>().Property(m => m.UserName)
                    .Apply(new DisplayAttribute() { Name = T["E-mail address"] })
                    .Apply(new RequiredAttribute().Localize());

                builder.Model<LoginModel>().Property(m => m.Password)
                    .Apply(new DisplayAttribute() { Name = T["Password"] })
                    .Apply(new RequiredAttribute().Localize())
                    .Apply(new DataTypeAttribute(DataType.Password));

                builder.Model<LoginModel>().Property(m => m.RememberMe)
                    .Apply(new DisplayAttribute() { Name = T["Remember me?"] });
            }
        }

        public bool RememberMe { get; set; }
    }

    [HandledAs(typeof(CreateUserCommand))]
    public class RegisterModel : CreateUserCommand
    {
        public class Configurer : ModelAttributesProviderConfigurer
        {
            public override void Configure(ModelAttributesProviderBuilder builder)
            {
                builder.Model<RegisterModel>().Property(m => m.UserName)
                    .Apply(new DisplayAttribute() { Name = T["E-mail address"] })
                    .Apply(new RequiredAttribute().Localize())
                    .Apply(new EmailAddressAttribute().Localize())
                    .Apply(new MaxLengthAttribute(320).Localize());

                builder.Model<RegisterModel>().Property(m => m.Password)
                    .Apply(new DisplayAttribute() { Name = T["Password"] })
                    .Apply(new RequiredAttribute().Localize())
                    .Apply(new DataTypeAttribute(DataType.Password))
                    .Apply(new StringLengthAttribute(100) { MinimumLength = 6, ErrorMessage = T["The {0} must be at least {2} characters long."] });

                builder.Model<RegisterModel>().Property(m => m.ConfirmPassword)
                    .Apply(new DisplayAttribute() { Name = T["Confirm password"] })
                    .Apply(new DataTypeAttribute(DataType.Password))
                    .Apply(new CompareAttribute(nameof(Password)) { ErrorMessage = T["The password and confirmation password must match."] });

                builder.Model<RegisterModel>().Property(m => m.FirstName)
                    .Apply(new DisplayAttribute() { Name = T["First name"] })
                    .Apply(new RequiredAttribute().Localize())
                    .Apply(new MaxLengthAttribute(100).Localize());

                builder.Model<RegisterModel>().Property(m => m.LastName)
                    .Apply(new DisplayAttribute() { Name = T["Last name"] })
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
            public override void Configure(ModelAttributesProviderBuilder builder)
            {
                builder.Model<ResetPasswordModel>().Property(m => m.UserName)
                    .Apply(new DisplayAttribute() { Name = T["E-mail address"] })
                    .Apply(new RequiredAttribute().Localize());
            }
        }

        public bool? Success { get; set; }
    }

    [HandledAs(typeof(ChangePasswordCommand))]
    public class SetPasswordModel : ChangePasswordCommand
    {
        public class Configurer : ModelAttributesProviderConfigurer
        {
            public override void Configure(ModelAttributesProviderBuilder builder)
            {
                builder.Model<SetPasswordModel>().Property(m => m.NewPassword)
                    .Apply(new DisplayAttribute() { Name = T["New password"] })
                    .Apply(new RequiredAttribute().Localize())
                    .Apply(new DataTypeAttribute(DataType.Password))
                    .Apply(new StringLengthAttribute(100) { MinimumLength = 6, ErrorMessage = T["The {0} must be at least {2} characters long."] });

                builder.Model<SetPasswordModel>().Property(m => m.ConfirmPassword)
                    .Apply(new DisplayAttribute() { Name = T["Confirm password"] })
                    .Apply(new DataTypeAttribute(DataType.Password))
                    .Apply(new CompareAttribute(nameof(NewPassword)) { ErrorMessage = T["The password and confirmation password must match."] });
            }
        }

        public string ConfirmPassword { get; set; }
        public bool? Success { get; set; }
    }
}
