using System.Linq;
using System.Data.Entity.Migrations;
using System.IO;
using System.Data.Entity.Migrations.Infrastructure;

namespace AspNetSkeleton.DeployTools.Migrations
{
    public abstract class DbMigrationBase : DbMigration
    {
        void ExecuteScript(bool up)
        {
            var metadata = (IMigrationMetadata)this;

            var scriptFile = Directory.EnumerateFiles(
                Path.Combine(Program.AssemblyPath, "Migrations"),
                $"{metadata.Id}.{(up ? "up" : "down")}.sql", 
                SearchOption.TopDirectoryOnly).FirstOrDefault();

            if (scriptFile != null)
                SqlFile(scriptFile);
        }

        public override void Up()
        {
            ExecuteScript(up: true);
        }

        public override void Down()
        {
            ExecuteScript(up: false);
        }
    }
}
