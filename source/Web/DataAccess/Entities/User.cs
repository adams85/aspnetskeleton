using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AspNetSkeleton.DataAccess.Entities
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(320)]
        public string UserName { get; set; }

        [Required]
        [StringLength(320)]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = true), DataType(DataType.Password)]
        [StringLength(172)]
        public string Password { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(200)]
        public string Comment { get; set; }

        public bool IsApproved { get; set; }
        public int PasswordFailuresSinceLastSuccess { get; set; }
        public DateTime? LastPasswordFailureDate { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public DateTime? LastLockoutDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        [StringLength(172)]
        public string ConfirmationToken { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime LastPasswordChangedDate { get; set; }
        [StringLength(172)]
        public string PasswordVerificationToken { get; set; }
        public DateTime? PasswordVerificationTokenExpirationDate { get; set; }

        ICollection<UserRole> _roles;
        public virtual ICollection<UserRole> Roles
        {
            get => _roles ?? (_roles = new HashSet<UserRole>());
            set => _roles = value;
        }

        public virtual Profile Profile { get; set; }
    }
}