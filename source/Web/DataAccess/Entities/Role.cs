using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AspNetSkeleton.DataAccess.Entities
{
    public class Role
    {
        public int RoleId { get; set; }

        [Required]
        [StringLength(32)]
        public string RoleName { get; set; }

        [StringLength(256)]
        public string Description { get; set; }

        ICollection<UserRole> _users;
        public virtual ICollection<UserRole> Users
        {
            get => _users ?? (_users = new HashSet<UserRole>());
            set => _users = value;
        }
    }
}