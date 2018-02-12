using System;
using AspNetSkeleton.Service.Contract.DataObjects;

namespace AspNetSkeleton.Service.Contract.Commands
{
    public class CreateNotificationCommand : IKeyGeneratorCommand
    {
        public string Code { get; set; }
        public string Data { get; set; }
        public Action<ICommand, object> OnKeyGenerated { get; set; }
    }

    public class DeleteNotificationCommand : ICommand
    {
        public int Id { get; set; }
    }

    public class MarkNotificationsCommand : ICommand
    {
        public int? Id { get; set; }
        public NotificationState? State { get; set; }
        public NotificationState NewState { get; set; }
        public int? Count { get; set; }
    }
}
