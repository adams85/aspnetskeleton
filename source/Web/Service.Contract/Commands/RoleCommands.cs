using System;
using AspNetSkeleton.Common;

namespace AspNetSkeleton.Service.Contract.Commands
{
    public class CreateRoleCommand : IKeyGeneratorCommand
    {
        public string RoleName { get; set; }

        public Action<ICommand, Polymorph<object>> OnKeyGenerated { get; set; }
    }

    public class DeleteRoleCommand : ICommand
    {
        public string RoleName { get; set; }
        public bool DeletePopulatedRole { get; set; }
    }

    public class AddUsersToRolesCommand : ICommand
    {
        public string[] UserNames { get; set; }
        public string[] RoleNames { get; set; }
    }

    public class RemoveUsersFromRolesCommand : ICommand
    {
        public string[] UserNames { get; set; }
        public string[] RoleNames { get; set; }
    }
}
