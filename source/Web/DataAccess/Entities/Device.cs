using LinqToDB.Mapping;
using System;

namespace AspNetSkeleton.DataAccess.Entities
{
    [Table]
    public class Device
    {
        [Column, PrimaryKey(0)]
        public IdentityKey<int> UserId { get; set; }

        [Column(Length = 172), PrimaryKey(1)]
        public string DeviceId { get; set; }

        [Column]
        public DateTime ConnectedAt { get; set; }

        [Column]
        public DateTime UpdatedAt { get; set; }

        [Column(Length = 20), NotNull]
        public string DeviceName { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(Entities.Profile.UserId), CanBeNull = false, Relationship = Relationship.ManyToOne, KeyName = "FK_Device_Profile_UserId", BackReferenceName = nameof(Entities.Profile.Devices))]
        public Profile Profile { get; set; }
    }
}