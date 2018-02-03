using System;

namespace AspNetSkeleton.Service.Contract.DataObjects
{
    public class UserData
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsLockedOut { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastPasswordChangedDate { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime? LastLockoutDate { get; set; }
    }

    public class AccountInfoData
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string[] Roles { get; set; }
    }

    public enum AuthenticateUserStatus
    {
        Unknown,
        Failed,
        Unapproved,
        LockedOut,
        Successful,
    }

    public class AuthenticateUserResult
    {
        public int? UserId { get; set; }
        public AuthenticateUserStatus Status { get; set; }
    }
}
