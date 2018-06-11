using System;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.Common.Utils;
using AspNetSkeleton.Common;

namespace AspNetSkeleton.Service.Contract.DataObjects
{
    [Flags]
    public enum NotificationState
    {
        Unknown = 0,
        Queued = 1,
        Processing = 2,
        Failed = 4,
    }

    public class NotificationData
    {
        public int Id { get; set; }
        public NotificationState State { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Code { get; set; }
        public string Data { get; set; }
    }

    public interface INotificationArgs
    {
        string Code { get; }
        string Serialize();
        void Deserialize(string data);
    }

    public abstract class NotificationArgs : INotificationArgs
    {
        protected abstract string CodeInternal { get; }

        string INotificationArgs.Code => CodeInternal;

        public string Serialize()
        {
            return SerializationUtils.SerializeObject(this);
        }

        public void Deserialize(string data)
        {
            SerializationUtils.PopulateObject(this, data);
        }

        public CreateNotificationCommand ToCommand()
        {
            return new CreateNotificationCommand
            {
                Code = CodeInternal,
                Data = Polymorph.Create<object>(Serialize()),
            };
        }
    }
}
