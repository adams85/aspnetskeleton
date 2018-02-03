using System;
using AspNetSkeleton.Service.Contract.DataObjects;

namespace AspNetSkeleton.Service.Contract.Queries
{
    public enum UserIdentifier
    {
        Id,
        Name,
        Email,
    }

    public class GetUserQuery : IQuery<UserData>
    {
        public UserIdentifier Identifier { get; set; }
        public object Key { get; set; }
    }

    public class ListUsersQuery : ListQuery<UserData>
    {
        public string UserNamePattern { get; set; }
        public string EmailPattern { get; set; }
        public string RoleName { get; set; }
    }

    public class GetOnlineUserCountQuery : IQuery<int>
    {
        public DateTime DateFrom { get; set; }
    }

    public class AuthenticateUserQuery : IQuery<AuthenticateUserResult>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class GetAccountInfoQuery : IQuery<AccountInfoData>
    {
        public string UserName { get; set; }
    }
}
