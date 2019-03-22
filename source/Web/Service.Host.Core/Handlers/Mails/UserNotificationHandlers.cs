using System.Collections.Generic;
using Karambolo.Common;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Service.Contract.DataObjects;
using RazorLight;
using Microsoft.Extensions.Options;

namespace AspNetSkeleton.Service.Host.Core.Handlers.Mails
{
    [HandlerFor(UnapprovedUserCreatedNotificationArgs.Code)]
    public class UnapprovedUserCreatedNotificationHandler : NotificationHandler<UnapprovedUserCreatedNotificationArgs>
    {
        readonly ServiceHostCoreSettings _settings;

        public UnapprovedUserCreatedNotificationHandler(IRazorLightEngine razorEngine, IOptions<ServiceHostCoreSettings> settings)
            : base(razorEngine)
        {
            _settings = settings.Value;
        }

        protected override UnapprovedUserCreatedNotificationArgs CreateModel(string data)
        {
            var result = new UnapprovedUserCreatedNotificationArgs();
            result.Deserialize(data);
            return result;
        }

        protected override string GetSender(UnapprovedUserCreatedNotificationArgs model)
        {
            return _settings.MailFrom;
        }

        protected override IEnumerable<string> GetTo(UnapprovedUserCreatedNotificationArgs model)
        {
            return new[] { model.Email };
        }

        protected override string GenerateSubject(UnapprovedUserCreatedNotificationArgs model)
        {
            return "Account Verification";
        }

        protected override bool IsBodyHtml => true;
    }

    [HandlerFor(PasswordResetNotificationArgs.Code)]
    public class PasswordResetNotificationHandler : NotificationHandler<PasswordResetNotificationArgs>
    {
        readonly ServiceHostCoreSettings _settings;

        public PasswordResetNotificationHandler(IRazorLightEngine razorEngine, IOptions<ServiceHostCoreSettings> settings)
            : base(razorEngine)
        {
            _settings = settings.Value;
        }

        protected override PasswordResetNotificationArgs CreateModel(string data)
        {
            var result = new PasswordResetNotificationArgs();
            result.Deserialize(data);
            return result;
        }

        protected override string GetSender(PasswordResetNotificationArgs model)
        {
            return _settings.MailFrom;
        }

        protected override IEnumerable<string> GetTo(PasswordResetNotificationArgs model)
        {
            return new[] { model.Email };
        }

        protected override string GenerateSubject(PasswordResetNotificationArgs model)
        {
            return "Password Reset";
        }

        protected override bool IsBodyHtml => true;
    }

    [HandlerFor(UserLockedOutNotificationArgs.Code)]
    public class UserLockedOutNotificationHandler : NotificationHandler<UserLockedOutNotificationArgs>
    {
        readonly ServiceHostCoreSettings _settings;

        public UserLockedOutNotificationHandler(IRazorLightEngine razorEngine, IOptions<ServiceHostCoreSettings> settings)
            : base(razorEngine)
        {
            _settings = settings.Value;
        }

        protected override UserLockedOutNotificationArgs CreateModel(string data)
        {
            var result = new UserLockedOutNotificationArgs();
            result.Deserialize(data);
            return result;
        }

        protected override string GetSender(UserLockedOutNotificationArgs model)
        {
            return _settings.MailFrom;
        }

        protected override IEnumerable<string> GetTo(UserLockedOutNotificationArgs model)
        {
            return new[] { model.Email };
        }

        protected override string GenerateSubject(UserLockedOutNotificationArgs model)
        {
            return "Account Lockout";
        }

        protected override bool IsBodyHtml => true;
    }
}