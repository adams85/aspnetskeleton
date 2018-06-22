using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.Service.Contract.Queries;
using AspNetSkeleton.UI.Infrastructure.Models;
using System.ComponentModel.DataAnnotations;
using AspNetSkeleton.UI.Infrastructure.Localization;

namespace AspNetSkeleton.UI.Models
{
    [HandledAs(typeof(AuthenticateUserQuery))]
    public class LoginModel : AuthenticateUserQuery
    {
        public class Configurer : ModelMetadataConfigurer
        {
            protected override void Configure(IModelMetadataBuilder builder)
            {
                builder.Model<LoginModel>().Property(m => m.UserName)
                    .DisplayName(() => T["E-mail address"])
                    .Validator(new RequiredAttribute().Localize());

                builder.Model<LoginModel>().Property(m => m.Password)
                    .DisplayName(() => T["Password"])
                    .Validator(new RequiredAttribute().Localize())
                    .Validator(new DataTypeAttribute(DataType.Password));

                builder.Model<LoginModel>().Property(m => m.RememberMe)
                    .DisplayName(() => T["Remember me?"]);
            }
        }

        public bool RememberMe { get; set; }
    }

    [HandledAs(typeof(CreateUserCommand))]
    public class RegisterModel : CreateUserCommand
    {
        public class Configurer : ModelMetadataConfigurer
        {
            protected override void Configure(IModelMetadataBuilder builder)
            {
                builder.Model<RegisterModel>().Property(m => m.UserName)
                    .DisplayName(() => T["E-mail address"])
                    .Validator(new RequiredAttribute().Localize())
                    .Validator(new EmailAddressAttribute().Localize())
                    .Validator(new MaxLengthAttribute(320).Localize());

                builder.Model<RegisterModel>().Property(m => m.Password)
                    .DisplayName(() => T["Password"])
                    .Validator(new RequiredAttribute().Localize())
                    .Validator(new DataTypeAttribute(DataType.Password))
                    .Validator(new StringLengthAttribute(100) { MinimumLength = 6, ErrorMessage = T["The {0} must be at least {2} characters long."] });

                builder.Model<RegisterModel>().Property(m => m.ConfirmPassword)
                    .DisplayName(() => T["Confirm password"])
                    .Validator(new DataTypeAttribute(DataType.Password))
                    .Validator(new CompareAttribute(nameof(Password)) { ErrorMessage = T["The password and confirmation password must match."] });

                builder.Model<RegisterModel>().Property(m => m.FirstName)
                    .DisplayName(() => T["First name"])
                    .Validator(new RequiredAttribute().Localize())
                    .Validator(new MaxLengthAttribute(100).Localize());

                builder.Model<RegisterModel>().Property(m => m.LastName)
                    .DisplayName(() => T["Last name"])
                    .Validator(new RequiredAttribute().Localize())
                    .Validator(new MaxLengthAttribute(100).Localize());
            }
        }

        public string ConfirmPassword { get; set; }
    }

    [HandledAs(typeof(ResetPasswordCommand))]
    public class ResetPasswordModel : ResetPasswordCommand
    {
        public class Configurer : ModelMetadataConfigurer
        {
            protected override void Configure(IModelMetadataBuilder builder)
            {
                builder.Model<ResetPasswordModel>().Property(m => m.UserName)
                    .DisplayName(() => T["E-mail address"])
                    .Validator(new RequiredAttribute().Localize());
            }
        }

        public bool? Success { get; set; }
    }

    [HandledAs(typeof(ChangePasswordCommand))]
    public class SetPasswordModel : ChangePasswordCommand
    {
        public class Configurer : ModelMetadataConfigurer
        {
            protected override void Configure(IModelMetadataBuilder builder)
            {
                builder.Model<SetPasswordModel>().Property(m => m.NewPassword)
                    .DisplayName(() => T["New password"])
                    .Validator(new RequiredAttribute().Localize())
                    .Validator(new DataTypeAttribute(DataType.Password))
                    .Validator(new StringLengthAttribute(100) { MinimumLength = 6, ErrorMessage = T["The {0} must be at least {2} characters long."] });

                builder.Model<SetPasswordModel>().Property(m => m.ConfirmPassword)
                    .DisplayName(() => T["Confirm password"])
                    .Validator(new DataTypeAttribute(DataType.Password))
                    .Validator(new CompareAttribute(nameof(NewPassword)) { ErrorMessage = T["The password and confirmation password must match."] });
            }
        }

        public string ConfirmPassword { get; set; }
        public bool? Success { get; set; }
    }
}
