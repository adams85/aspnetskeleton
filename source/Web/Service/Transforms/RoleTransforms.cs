using System;
using System.Linq;
using AspNetSkeleton.DataAccess.Entities;
using System.Linq.Expressions;
using AspNetSkeleton.Service.Contract.DataObjects;
using System.Threading.Tasks;
using System.Threading;
using AspNetSkeleton.DataAccess;

namespace AspNetSkeleton.Service.Transforms
{
    public static class RoleTransforms
    {
        static readonly Expression<Func<Role, RoleData>> toDataExpr = r => new RoleData
        {
            RoleId = r.RoleId.Value,
            RoleName = r.RoleName,
            Description = r.Description,
        };

        static readonly Func<Role, RoleData> toData = toDataExpr.Compile();

        public static RoleData ToData(this Role entity)
        {
            return toData(entity);
        }

        public static IQueryable<RoleData> ToData(this IQueryable<Role> linq)
        {
            return linq.Select(toDataExpr);
        }

        public static Expression<Func<Role, bool>> GetFilterByNameWhere(string name, bool pattern = false)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (pattern)
                return r => r.RoleName.ToLower().Contains(name.ToLower());
            else
                return r => r.RoleName.ToLower() == name.ToLower();
        }

        public static IQueryable<Role> FilterByName(this IQueryable<Role> linq, string name, bool pattern = false)
        {
            return linq.Where(GetFilterByNameWhere(name, pattern));
        }

        public static Task<Role> GetByNameAsync(this IQueryable<Role> linq, string name, CancellationToken cancellationToken)
        {
            return linq.FilterByName(name).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
