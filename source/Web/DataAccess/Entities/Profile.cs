using LinqToDB.Mapping;
using System.Collections.Generic;

namespace AspNetSkeleton.DataAccess.Entities
{
    [Table]
    public class Profile
    {
        [Column, PrimaryKey]
        public IdentityKey<int> UserId { get; set; }

        [Column(Length = 100)]
        public string FirstName { get; set; }

        [Column(Length = 100)]
        public string LastName { get; set; }

        [Column(Length = 50)]
        public string PhoneNumber { get; set; }

        [Column]
        public int DeviceLimit { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(Entities.User.UserId), CanBeNull = false, Relationship = Relationship.OneToOne, KeyName = "FK_Profile_User_UserId", BackReferenceName = nameof(Entities.User.Profile))]
        public User User { get; set; }

        [Association(OtherKey = nameof(Device.UserId), ThisKey = nameof(UserId), Relationship = Relationship.OneToMany, IsBackReference = true)]
        public IEnumerable<Device> Devices { get; set; }
    }
}