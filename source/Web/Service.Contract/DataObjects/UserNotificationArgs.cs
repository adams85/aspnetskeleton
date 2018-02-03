using System;

namespace AspNetSkeleton.Service.Contract.DataObjects
{
    public class UnapprovedUserCreatedNotificationArgs : NotificationArgs
    {
        public const string Code = "UnapprovedUserCreated";

        public string Name { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string VerificationToken { get; set; }

        protected override string CodeInternal => Code;
    }

    public class PasswordResetNotificationArgs : NotificationArgs
    {
        public const string Code = "PasswordReset";

        public string Name { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string VerificationToken { get; set; }

        public DateTime VerificationTokenExpirationDate { get; set; }

        protected override string CodeInternal => Code;
    }

    public class UserLockedOutNotificationArgs : NotificationArgs
    {
        public const string Code = "UserLockedOut";

        public string Name { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        protected override string CodeInternal => Code;
    }
}
