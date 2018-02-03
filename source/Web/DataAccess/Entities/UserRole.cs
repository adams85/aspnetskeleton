using LinqToDB.Mapping;

namespace AspNetSkeleton.DataAccess.Entities
{
    [Table]
    public class UserRole
    {
        [Column, PrimaryKey(0)]
        public IdentityKey<int> UserId { get; set; }

        [Column, PrimaryKey(1)]
        public IdentityKey<int> RoleId { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(Entities.User.UserId), CanBeNull = false, Relationship = Relationship.ManyToOne, KeyName = "FK_UserRole_User_UserId", BackReferenceName = nameof(Entities.User.Roles))]
        public User User { get; set; }

        [Association(ThisKey = nameof(RoleId), OtherKey = nameof(Entities.Role.RoleId), CanBeNull = false, Relationship = Relationship.ManyToOne, KeyName = "FK_UserRole_Role_RoleId", BackReferenceName = nameof(Entities.Role.Users))]
        public Role Role { get; set; }
    }
}