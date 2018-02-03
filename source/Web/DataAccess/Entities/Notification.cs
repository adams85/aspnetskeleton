using System;
using AspNetSkeleton.Service.Contract.DataObjects;
using LinqToDB.Mapping;
using LinqToDB;

namespace AspNetSkeleton.DataAccess.Entities
{
    [Table]
    public class Notification
    {
        [Column, PrimaryKey, Identity]
        public IdentityKey<int> Id { get; set; }

        [Column]
        public NotificationState State { get; set; }

        [Column]
        public DateTime CreatedAt { get; set; }

        [Column(Length = 64), NotNull]
        public string Code { get; set; }

        [Column(DataType = DataType.Text), NotNull]
        public string Data { get; set; }
    }
}
