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
        public class Configurer : ModelAttributesProviderConfigurer
        {
            public override void Configure(ModelAttributesProviderBuilder builder)
            {
                builder.Model<ChangePasswordModel>().Property(m => m.CurrentPassword)
                    .Apply(new DisplayAttribute() { Name = T["Current password"] })
                    .Apply(new RequiredAttribute().Localize())
                    .Apply(new DataTypeAttribute(DataType.Password));

                builder.Model<ChangePasswordModel>().Property(m => m.NewPassword)
                    .Apply(new DisplayAttribute() { Name = T["New password"] })
                    .Apply(new RequiredAttribute().Localize())
                    .Apply(new DataTypeAttribute(DataType.Password))
                    .Apply(new StringLengthAttribute(100) { MinimumLength = 6, ErrorMessage = T["The {0} must be at least {2} characters long."] });

                builder.Model<ChangePasswordModel>().Property(m => m.ConfirmPassword)
                    .Apply(new DisplayAttribute() { Name = T["Confirm password"] })
                    .Apply(new DataTypeAttribute(DataType.Password))
                    .Apply(new CompareAttribute(nameof(NewPassword)) { ErrorMessage = T["The password and confirmation password must match."] });
            }
        }

        public string CurrentPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public bool? Success { get; set; }
    }
}