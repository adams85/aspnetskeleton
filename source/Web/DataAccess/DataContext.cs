using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using AspNetSkeleton.DataAccess.Entities;
using AspNetSkeleton.DataAccess.Entities.ComplexTypes;
using System.Data.Entity.Infrastructure.Annotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;

namespace AspNetSkeleton.DataAccess
{
    public class DataContext : DbContextBase<DataContext>
    {
        public DataContext() { }

        public DataContext(DbConnection connection, bool ownsConnection)
            : base(connection, ownsConnection) { }

        public DataContext(string nameOrConnectionString)
            : base(nameOrConnectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Properties<decimal>()
                .Configure(p => p.HasPrecision(DataAccessConstants.MoneyPrecision, DataAccessConstants.MoneyScale));

            #region Complex Types
            modelBuilder.ComplexType<DbMoney>()
                .Property(ma => ma.Currency)
                .HasColumnType("char")
                .HasMaxLength(3)
                .IsFixedLength();

            modelBuilder.ComplexType<DbCurrency>()
                .Property(ma => ma.Value)
                .HasColumnType("char")
                .HasMaxLength(3)
                .IsFixedLength();
            #endregion

            #region User
            modelBuilder.Entity<User>()
                .Property(u => u.UserName)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute() { IsUnique = true }));

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute() { IsUnique = true }));
            #endregion

            #region Role
            modelBuilder.Entity<Role>()
                .Property(r => r.RoleName)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute() { IsUnique = true }));
            #endregion

            #region UserRole
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasRequired(ur => ur.User)
                .WithMany(r => r.Roles)
                .HasForeignKey(ur => ur.UserId)
                .WillCascadeOnDelete();

            modelBuilder.Entity<UserRole>()
                .HasRequired(ur => ur.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(ur => ur.RoleId)
                .WillCascadeOnDelete();
            #endregion

            #region Profile
            modelBuilder.Entity<Profile>()
                .HasKey(p => p.UserId);

            modelBuilder.Entity<Profile>()
                .HasRequired(p => p.User)
                .WithOptional(u => u.Profile)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Profile>()
                .HasMany(p => p.Devices)
                .WithRequired(d => d.Profile)
                .HasForeignKey(d => d.UserId);
            #endregion

            #region Device
            modelBuilder.Entity<Device>()
                .HasKey(d => new { d.UserId, d.DeviceId });
            #endregion

            #region Notification
            modelBuilder.Entity<Notification>()
                .Property(m => m.CreatedAt)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute()));
            #endregion
        }
    }
}