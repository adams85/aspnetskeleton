using System;
using System.Web.Mvc;
using System.Web.Security;
using Karambolo.Common;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSkeleton.UI.Infrastructure.Security
{
    public class UIRoleProvider : RoleProvider
    {
        public override string ApplicationName
        {
            get => typeof(MvcApplication).Assembly.GetName().Name;
            set { throw new NotSupportedException(); }
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotSupportedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            throw new NotSupportedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotSupportedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotSupportedException();
        }

        public override string[] GetRolesForUser(string username)
        {
            var accountManager = DependencyResolver.Current.GetService<IAccountManager>();
            // HACK: execution must not be marshalled back to the original sync. context,
            // otherwise sync. wait in GetRolesForUser() will cause a dead-lock!!!
            // https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html
            var accountInfo = Task.Run(() => accountManager.GetAccountInfoAsync(username, registerActivity: true, cancellationToken: CancellationToken.None)).WaitAndUnwrap();

            return accountInfo.Roles;
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotSupportedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotSupportedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotSupportedException();
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotSupportedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotSupportedException();
        }
    }
}