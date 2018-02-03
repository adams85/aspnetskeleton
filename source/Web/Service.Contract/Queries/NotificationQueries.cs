using AspNetSkeleton.Service.Contract.DataObjects;

namespace AspNetSkeleton.Service.Contract.Queries
{
    public class ListNotificationsQuery : ListQuery<NotificationData>
    {
        public NotificationState? State { get; set; }
    }
}
