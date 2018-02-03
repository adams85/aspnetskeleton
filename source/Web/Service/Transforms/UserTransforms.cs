using System;
using System.Linq;
using AspNetSkeleton.DataAccess.Entities;
using System.Linq.Expressions;
using AspNetSkeleton.Service.Contract.DataObjects;
using System.Threading;
using System.Threading.Tasks;
using AspNetSkeleton.DataAccess;

namespace AspNetSkeleton.Service.Transforms
{
    public static class UserTransforms
    {
        static readonly Expression<Func<User, UserData>> toDataExpr = u => new UserData
        {
            UserId = u.UserId.Value,
            UserName = u.UserName,
            Email = u.Email,
            IsLockedOut = u.IsLockedOut,
            IsApproved = u.IsApproved,
            CreationDate = u.CreateDate,
            LastPasswordChangedDate = u.LastPasswordChangedDate,
            LastActivityDate = u.LastActivityDate,
            LastLoginDate = u.LastLoginDate,
            LastLockoutDate = u.LastLockoutDate,
        };

        static readonly Func<User, UserData> toData = toDataExpr.Compile();

        public static UserData ToData(this User entity)
        {
            return toData(entity);
        }

        public static IQueryable<UserData> ToData(this IQueryable<User> linq)
        {
            return linq.Select(toDataExpr);
        }

        public static Expression<Func<User, bool>> GetFilterByNameWhere(string name, bool pattern = false)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (pattern)
                return u => u.UserName.ToLower().Contains(name.ToLower());
            else
                return u => u.UserName.ToLower() == name.ToLower();
        }

        public static IQueryable<User> FilterByName(this IQueryable<User> linq, string name, bool pattern = false)
        {
            return linq.Where(GetFilterByNameWhere(name, pattern));
        }

        public static Task<User> GetByNameAsync(this IQueryable<User> linq, string name, CancellationToken cancellationToken)
        {
            return linq.FilterByName(name).FirstOrDefaultAsync(cancellationToken);
        }

        public static Expression<Func<User, bool>> GetFilterByEmailWhere(string email, bool pattern = false)
        {
            if (email == null)
                throw new ArgumentNullException(nameof(email));

            if (pattern)
                return u => u.Email.ToLower().Contains(email.ToLower());
            else
                return u => u.Email.ToLower() == email.ToLower();
        }

        public static IQueryable<User> FilterByEmail(this IQueryable<User> linq, string email, bool pattern = false)
        {
            return linq.Where(GetFilterByEmailWhere(email, pattern));
        }

        public static Task<User> GetByEmailAsync(this IQueryable<User> linq, string email, CancellationToken cancellationToken)
        {
            return linq.FilterByEmail(email).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
