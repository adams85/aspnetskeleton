using System;
using System.Linq;
using AspNetSkeleton.DataAccess.Entities;
using System.Linq.Expressions;
using AspNetSkeleton.Service.Contract.DataObjects;

namespace AspNetSkeleton.Service.Transforms
{
    public static class NotificationTransforms
    {
        static readonly Expression<Func<Notification, NotificationData>> toDataExpr = n => new NotificationData
        {
            Id = n.Id.Value,
            State = n.State,
            Code = n.Code,
            CreatedAt = n.CreatedAt,
            Data = n.Data,
        };

        static readonly Func<Notification, NotificationData> toData = toDataExpr.Compile();

        public static NotificationData ToData(this Notification entity)
        {
            return toData(entity);
        }

        public static IQueryable<NotificationData> ToData(this IQueryable<Notification> linq)
        {
            return linq.Select(toDataExpr);
        }
    }
}
