using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.Service.Contract.Queries;
using Karambolo.Common.Collections;
using System.Collections.Generic;
using AspNetSkeleton.Common.Infrastructure;
using Karambolo.Common;

namespace AspNetSkeleton.AdminTools.Operations.Users
{
    [HandlerFor(Name)]
    public class ListUsersOperation : ApiOperation
    {
        public const string Name = "list-users";

        static readonly IReadOnlyOrderedDictionary<string, ListColumnDef> columnDefs = new OrderedDictionary<string, ListColumnDef>
        {
            { "Id", new ListColumnDef { Width = 5 } },
            { "Name", new ListColumnDef { Width = 18 } },
            { "Email", new ListColumnDef { Width = 30 } },
            { "Approved", new ListColumnDef { Width = 8 } },
            { "LockOut", new ListColumnDef { Width = 8 } },
            { "CreationTime", new ListColumnDef { Width = 22 } },
            { "LastLoginTime", new ListColumnDef { Width = 22 } },
        };

        public ListUsersOperation(string[] args, IApiOperationContext context) : base(args, context) { }

        protected override int MandatoryArgCount => 0;

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name} [/n=<user-name-pattern>] [/e=<email-pattern>] [/r=<role-name>]";
        }

        protected override void ExecuteCore()
        {
            if (!OptionalArgs.TryGetValue("n", out string userNamePattern))
                userNamePattern = null;

            if (!OptionalArgs.TryGetValue("e", out string emailNamePattern))
                emailNamePattern = null;

            if (!OptionalArgs.TryGetValue("r", out string roleName))
                roleName = null;

            var result = Query(new ListUsersQuery
            {
                UserNamePattern = userNamePattern,
                EmailPattern = emailNamePattern,
                RoleName = roleName,
                OrderColumns = new[] { nameof(UserData.UserName) },
            });

            PrintList(columnDefs, result.Rows, r => new object[]
            {
                r.UserId,
                r.UserName,
                r.Email,
                r.IsApproved,
                r.IsLockedOut,
                r.CreationDate,
                r.LastLoginDate
            });
        }
    }
}
