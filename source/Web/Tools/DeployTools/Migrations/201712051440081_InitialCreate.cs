namespace AspNetSkeleton.DeployTools.Migrations
{

    public partial class InitialCreate : DbMigrationBase
    {
        public override void Up()
        {
            CreateTable(
                "dbo.User",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        UserName = c.String(nullable: false, maxLength: 320, storeType: "nvarchar"),
                        Email = c.String(nullable: false, maxLength: 320, storeType: "nvarchar"),
                        Password = c.String(nullable: false, maxLength: 172, storeType: "nvarchar"),
                        Comment = c.String(maxLength: 200, storeType: "nvarchar"),
                        IsApproved = c.Boolean(nullable: false),
                        PasswordFailuresSinceLastSuccess = c.Int(nullable: false),
                        LastPasswordFailureDate = c.DateTime(precision: 0),
                        LastActivityDate = c.DateTime(precision: 0),
                        LastLockoutDate = c.DateTime(precision: 0),
                        LastLoginDate = c.DateTime(precision: 0),
                        ConfirmationToken = c.String(maxLength: 172, storeType: "nvarchar"),
                        CreateDate = c.DateTime(nullable: false, precision: 0),
                        IsLockedOut = c.Boolean(nullable: false),
                        LastPasswordChangedDate = c.DateTime(nullable: false, precision: 0),
                        PasswordVerificationToken = c.String(maxLength: 172, storeType: "nvarchar"),
                        PasswordVerificationTokenExpirationDate = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.UserId)
                .Index(t => t.UserName, unique: true)
                .Index(t => t.Email, unique: true);
            
            CreateTable(
                "dbo.Profile",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        FirstName = c.String(maxLength: 100, storeType: "nvarchar"),
                        LastName = c.String(maxLength: 100, storeType: "nvarchar"),
                        PhoneNumber = c.String(maxLength: 50, storeType: "nvarchar"),
                        DeviceLimit = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Device",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        DeviceId = c.String(nullable: false, maxLength: 172, storeType: "nvarchar"),
                        ConnectedAt = c.DateTime(nullable: false, precision: 0),
                        UpdatedAt = c.DateTime(nullable: false, precision: 0),
                        DeviceName = c.String(maxLength: 20, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => new { t.UserId, t.DeviceId })
                .ForeignKey("dbo.Profile", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserRole",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.Role", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Role",
                c => new
                    {
                        RoleId = c.Int(nullable: false, identity: true),
                        RoleName = c.String(nullable: false, maxLength: 32, storeType: "nvarchar"),
                        Description = c.String(maxLength: 256, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.RoleId)
                .Index(t => t.RoleName, unique: true);
            
            CreateTable(
                "dbo.Notification",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        State = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false, precision: 0),
                        Code = c.String(nullable: false, maxLength: 64, storeType: "nvarchar"),
                        Data = c.String(nullable: false, unicode: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.CreatedAt);

            base.Up();
        }
        
        public override void Down()
        {
            base.Down();

            DropForeignKey("dbo.UserRole", "UserId", "dbo.User");
            DropForeignKey("dbo.UserRole", "RoleId", "dbo.Role");
            DropForeignKey("dbo.Profile", "UserId", "dbo.User");
            DropForeignKey("dbo.Device", "UserId", "dbo.Profile");
            DropIndex("dbo.Notification", new[] { "CreatedAt" });
            DropIndex("dbo.Role", new[] { "RoleName" });
            DropIndex("dbo.UserRole", new[] { "RoleId" });
            DropIndex("dbo.UserRole", new[] { "UserId" });
            DropIndex("dbo.Device", new[] { "UserId" });
            DropIndex("dbo.Profile", new[] { "UserId" });
            DropIndex("dbo.User", new[] { "Email" });
            DropIndex("dbo.User", new[] { "UserName" });
            DropTable("dbo.Notification");
            DropTable("dbo.Role");
            DropTable("dbo.UserRole");
            DropTable("dbo.Device");
            DropTable("dbo.Profile");
            DropTable("dbo.User");
        }
    }
}
