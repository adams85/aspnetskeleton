using System.Collections.Generic;
using RazorEngine.Templating;
using Karambolo.Common;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.Service.Host.Core.Infrastructure;

namespace AspNetSkeleton.Service.Host.Core.Handlers.Mails
{
    [HandlerFor(UnapprovedUserCreatedNotificationArgs.Code)]
    public class UnapprovedUserCreatedNotificationHandler : NotificationHandler<UnapprovedUserCreatedNotificationArgs>
    {
        readonly IServiceHostCoreSettings _settings;

        public UnapprovedUserCreatedNotificationHandler(IServiceHostEnvironment hostEnvironment, IRazorEngineService razorEngine, IServiceHostCoreSettings settings)
            : base(hostEnvironment, razorEngine)
        {
            _settings = settings;
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
            return EnumerableUtils.FromElement(model.Email);
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
        readonly IServiceHostCoreSettings _settings;

        public PasswordResetNotificationHandler(IServiceHostEnvironment hostEnvironment, IRazorEngineService razorEngine, IServiceHostCoreSettings settings)
            : base(hostEnvironment, razorEngine)
        {
            _settings = settings;
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
            return EnumerableUtils.FromElement(model.Email);
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
        readonly IServiceHostCoreSettings _settings;

        public UserLockedOutNotificationHandler(IServiceHostEnvironment hostEnvironment, IRazorEngineService razorEngine, IServiceHostCoreSettings settings)
            : base(hostEnvironment, razorEngine)
        {
            _settings = settings;
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
            return EnumerableUtils.FromElement(model.Email);
        }

        protected override string GenerateSubject(UserLockedOutNotificationArgs model)
        {
            return "Account Lockout";
        }

        protected override bool IsBodyHtml => true;
    }
}