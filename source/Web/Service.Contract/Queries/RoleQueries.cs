using AspNetSkeleton.Service.Contract.DataObjects;

namespace AspNetSkeleton.Service.Contract.Queries
{
    public enum RoleIdentifier
    {
        Id,
        Name,
    }

    public class GetRoleQuery : IQuery<RoleData>
    {
        public RoleIdentifier Identifier { get; set; }
        public object Key { get; set; }
    }

    public class ListRolesQuery : ListQuery<RoleData>
    {
        public string UserName { get; set; }
    }

    public class IsUserInRoleQuery : IQuery<bool>
    {
        public string UserName { get; set; }
        public string RoleName { get; set; }
    }
}
