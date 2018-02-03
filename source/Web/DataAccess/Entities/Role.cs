using LinqToDB.Mapping;
using System.Collections.Generic;

namespace AspNetSkeleton.DataAccess.Entities
{
    [Table]
    public class Role
    {
        [Column, PrimaryKey, Identity]
        public IdentityKey<int> RoleId { get; set; }

        [Column(Length = 32), NotNull]
        public string RoleName { get; set; }

        [Column(Length = 256)]
        public string Description { get; set; }

        [Association(OtherKey = nameof(UserRole.RoleId), ThisKey = nameof(RoleId), Relationship = Relationship.OneToMany, IsBackReference = true)]
        public IEnumerable<UserRole> Users { get; set; }
    }
}