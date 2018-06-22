using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.UI.Infrastructure.Models;
using System.ComponentModel.DataAnnotations;
using AspNetSkeleton.UI.Infrastructure.Localization;

namespace AspNetSkeleton.UI.Areas.Dashboard.Models
{
    [HandledAs(typeof(ChangePasswordCommand))]
    public class ChangePasswordModel : ChangePasswordCommand
    {
        public class Configurer : ModelMetadataConfigurer
        {
            protected override void Configure(IModelMetadataBuilder builder)
            {
                builder.Model<ChangePasswordModel>().Property(m => m.CurrentPassword)
                    .DisplayName(() => T["Current password"])
                    .Validator(new RequiredAttribute().Localize())
                    .Validator(new DataTypeAttribute(DataType.Password));

                builder.Model<ChangePasswordModel>().Property(m => m.NewPassword)
                    .DisplayName(() => T["New password"])
                    .Validator(new RequiredAttribute().Localize())
                    .Validator(new DataTypeAttribute(DataType.Password))
                    .Validator(new StringLengthAttribute(100) { MinimumLength = 6, ErrorMessage = T["The {0} must be at least {2} characters long."] });

                builder.Model<ChangePasswordModel>().Property(m => m.ConfirmPassword)
                    .DisplayName(() => T["Confirm password"])
                    .Validator(new DataTypeAttribute(DataType.Password))
                    .Validator(new CompareAttribute(nameof(NewPassword)) { ErrorMessage = T["The password and confirmation password must match."] });
            }
        }

        public string CurrentPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public bool? Success { get; set; }
    }
}