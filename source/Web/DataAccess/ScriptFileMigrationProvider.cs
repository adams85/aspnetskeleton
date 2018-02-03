using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSkeleton.DataAccess
{
    public class ScriptFileMigrationProvider : IDbMigrationProvider
    {
        readonly string _basePath;

        public ScriptFileMigrationProvider(string basePath)
        {
            if (basePath == null)
                throw new ArgumentNullException(nameof(basePath));

            _basePath = basePath;
        }

        string BuildScriptFilePath(DbContext context, string migration, bool commit)
        {
            return Path.Combine(_basePath, context.GetType().Name, $"{context.ProviderName}.{migration}.{(commit ? "Up" : "Down")}.sql");
        }

        public Task<string> GetCommitScriptAsync(DbContext context, string migration, CancellationToken cancellationToken)
        {
            var filePath = BuildScriptFilePath(context, migration, commit: true);
            return File.ReadAllTextAsync(filePath);
        }

        public Task<string> GetRevertScriptAsync(DbContext context, string migration, CancellationToken cancellationToken)
        {
            var filePath = BuildScriptFilePath(context, migration, commit: false);
            return File.ReadAllTextAsync(filePath);
        }
    }
}
