using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.Service.Contract.Queries;
using Karambolo.Common;
using Karambolo.Common.Collections;
using System.Collections.Generic;

namespace AspNetSkeleton.AdminTools.Operations.Roles
{
    [HandlerFor(Name)]
    public class ListRolesOperation : ApiOperation
    {
        public const string Name = "list-roles";

        static readonly IReadOnlyOrderedDictionary<string, ListColumnDef> columnDefs = new OrderedDictionary<string, ListColumnDef>
        {
            { "Id", new ListColumnDef { Width = 5 } },
            { "Name", new ListColumnDef { Width = 32 } },
        };

        public ListRolesOperation(string[] args, IApiOperationContext context) : base(args, context) { }

        protected override int MandatoryArgCount => 0;

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name} [/u=<user-name>]";
        }        

        protected override void ExecuteCore()
        {
            if (!OptionalArgs.TryGetValue("u", out string userName))
                userName = null;

            var result = Query(new ListRolesQuery
            {
                UserName = userName,
                OrderColumns = new[] { nameof(RoleData.RoleName) },
            });

            PrintList(columnDefs, result.Rows, r => new object[]
            {
                r.RoleId,
                r.RoleName
            });
        }
    }
}
