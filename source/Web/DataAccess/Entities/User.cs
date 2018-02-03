using LinqToDB.Mapping;
using System;
using System.Collections.Generic;

namespace AspNetSkeleton.DataAccess.Entities
{
    [Table]
    public class User
    {
        [Column, PrimaryKey, Identity]
        public IdentityKey<int> UserId { get; set; }

        [Column(Length = 320), NotNull]
        public string UserName { get; set; }

        [Column(Length = 320), NotNull]
        public string Email { get; set; }

        [Column(Length = 172), NotNull]
        public string Password { get; set; }

        [Column(Length = 200)]
        public string Comment { get; set; }

        [Column]
        public bool IsApproved { get; set; }

        [Column]
        public int PasswordFailuresSinceLastSuccess { get; set; }

        [Column]
        public DateTime? LastPasswordFailureDate { get; set; }

        [Column]
        public DateTime? LastActivityDate { get; set; }

        [Column]
        public DateTime? LastLockoutDate { get; set; }

        [Column]
        public DateTime? LastLoginDate { get; set; }

        [Column(Length = 172)]
        public string ConfirmationToken { get; set; }

        [Column]
        public DateTime CreateDate { get; set; }

        [Column]
        public bool IsLockedOut { get; set; }

        [Column]
        public DateTime LastPasswordChangedDate { get; set; }

        [Column(Length = 172)]
        public string PasswordVerificationToken { get; set; }

        [Column]
        public DateTime? PasswordVerificationTokenExpirationDate { get; set; }

        [Association(OtherKey = nameof(UserRole.UserId), ThisKey = nameof(UserId), Relationship = Relationship.OneToMany, IsBackReference = true)]
        public IEnumerable<UserRole> Roles { get; set; }

        [Association(OtherKey = nameof(Entities.Profile.UserId), ThisKey = nameof(UserId), Relationship = Relationship.OneToOne, IsBackReference = true)]
        public Profile Profile { get; set; }
    }
}