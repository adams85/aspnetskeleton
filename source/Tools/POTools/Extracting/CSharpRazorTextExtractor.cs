using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Razor;
using System.Web.Razor.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CSharp;

namespace AspNetSkeleton.POTools.Extracting
{
    public class CSharpRazorTextExtractor : CSharpTextExtractor
    {
        readonly RazorTemplateEngine _templateEngine;
        readonly CSharpCodeProvider _codeProvider;

        public CSharpRazorTextExtractor() : this(null) { }

        public CSharpRazorTextExtractor(CSharpTextExtractorSettings settings) : base(settings)
        {
            var engineHost = new RazorEngineHost(RazorCodeLanguage.Languages["cshtml"])
            {
                GeneratedClassContext = new GeneratedClassContext(
                    GeneratedClassContext.DefaultExecuteMethodName,
                    GeneratedClassContext.DefaultWriteMethodName,
                    GeneratedClassContext.DefaultWriteLiteralMethodName,
                    "WriteTo", "WriteLiteralTo", "Template", "DefineSection", "BeginContext", "EndContext")
            };

            _templateEngine = new RazorTemplateEngine(engineHost);

            _codeProvider = new CSharpCodeProvider();
        }

        protected override string GetCode(string content, CancellationToken cancellationToken)
        {
            GeneratorResults results;
            using (var reader = new StringReader(content))
                results = _templateEngine.GenerateCode(reader, null, null, "_", cancellationToken);

            if (!results.Success)
                throw new ArgumentException("Razor code has errors.", nameof(content));

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
                _codeProvider.GenerateCodeFromCompileUnit(results.GeneratedCode, writer, null);

            return sb.ToString();
        }

        protected override IEnumerable<SyntaxNode> GetRootNodes(SyntaxTree syntaxTree, CancellationToken cancellationToken)
        {
            return syntaxTree.GetRoot(cancellationToken).DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(md => !md.Modifiers.Any(sk => sk.IsKind(SyntaxKind.StaticKeyword)));
        }
    }
}
