using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AspNetSkeleton.POTools.Extracting
{
    public class CSharpRazorTextExtractor : CSharpTextExtractor
    {
        readonly RazorTemplateEngine _templateEngine;

        public CSharpRazorTextExtractor() : this(null) { }

        public CSharpRazorTextExtractor(CSharpTextExtractorSettings settings) : base(settings)
        {
            var projectEngine = RazorProjectEngine.Create(RazorConfiguration.Default, RazorProjectFileSystem.Create(@"\"));
            _templateEngine = new RazorTemplateEngine(projectEngine.Engine, projectEngine.FileSystem);
        }

        protected override string GetCode(string content, CancellationToken cancellationToken)
        {
            var document = RazorCodeDocument.Create(RazorSourceDocument.Create(content, "_"));
            var parsedDocument = _templateEngine.GenerateCode(document);
            if (parsedDocument.Diagnostics.OfType<RazorDiagnostic>().Any(d => d.Severity == RazorDiagnosticSeverity.Error))
                throw new ArgumentException("Razor code has errors.", nameof(content));

            return parsedDocument.GeneratedCode;
        }

        protected override IEnumerable<SyntaxNode> GetRootNodes(SyntaxTree syntaxTree, CancellationToken cancellationToken)
        {
            return syntaxTree.GetRoot(cancellationToken).DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(md => !md.Modifiers.Any(sk => sk.IsKind(SyntaxKind.StaticKeyword)));
        }
    }
}
