namespace AspNetSkeleton.DeployTools.Migrations
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<AspNetSkeleton.DataAccess.DataContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        public Configuration(DbConnectionInfo connInfo) : this()
        {
            TargetDatabase = connInfo;
        }

        protected override void Seed(AspNetSkeleton.DataAccess.DataContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
