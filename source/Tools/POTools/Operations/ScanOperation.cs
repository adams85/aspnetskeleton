using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;
using Karambolo.Common;

namespace AspNetSkeleton.POTools.Operations
{
    [HandlerFor(Name)]
    class ScanOperation : Operation
    {
        static readonly HashSet<string> projectExtensionFilter = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".csproj" };

        static readonly HashSet<string> compileExtensionFilter = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".cs" };
        static readonly HashSet<string> contentExtensionFilter = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".cshtml" };
        static readonly HashSet<string> extensionFilter = compileExtensionFilter.Concat(contentExtensionFilter).ToHashSet(StringComparer.OrdinalIgnoreCase);

        public const string Name = "scan";
        public const string Hint = "Scans for source files.";

        public ScanOperation(string[] args, IOperationContext context) : base(args, context) { }

        protected override int MandatoryArgCount => 0;

        public override void Execute()
        {
            bool isMSBuildFile;
            if (!OptionalArgs.TryGetValue("p", out string path))
            {
                path = Directory.GetCurrentDirectory();

                var projectFiles = Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly)
                    .Where(p => projectExtensionFilter.Contains(Path.GetExtension(p)))
                    .Take(2)
                    .ToArray();

                if (projectFiles.Length == 1)
                {
                    path = projectFiles[0];
                    isMSBuildFile = true;
                }
                else
                    isMSBuildFile = false;
            }
            else
                isMSBuildFile = !Directory.Exists(path);

            IEnumerable<string> filePaths;
            if (isMSBuildFile)
            {
                var projectCollection = new Microsoft.Build.Evaluation.ProjectCollection();
                var project = new Microsoft.Build.Evaluation.Project(path, null, null, projectCollection, Microsoft.Build.Evaluation.ProjectLoadSettings.IgnoreMissingImports);

                var basePath = Path.GetDirectoryName(path);

                filePaths = project.GetItemsIgnoringCondition("Compile").Where(pi => compileExtensionFilter.Contains(Path.GetExtension(pi.EvaluatedInclude)))
                    .Concat(project.GetItemsIgnoringCondition("Content").Where(pi => contentExtensionFilter.Contains(Path.GetExtension(pi.EvaluatedInclude))))
                    .Select(pi => Path.Combine(basePath, pi.EvaluatedInclude));
            }
            else
                filePaths = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                    .Where(p => extensionFilter.Contains(Path.GetExtension(p)));

            foreach (var filePath in filePaths)
                Context.Out.WriteLine(filePath);
        }

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name} [/p=<path>]";
            yield return Hint;
            yield return "  path: A path to an MSBuild file or a directory to look for source files for. If omitted, the project file in the current directory or the current directory if no or multiple project files exist. " +
                "(In the case of an MSBuild file, application should be run from a VS command prompt or VSINSTALLDIR and VisualStudioVersion environment variables must be set!)";
        }
    }
}
