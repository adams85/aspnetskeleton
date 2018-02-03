using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AspNetSkeleton.POTools.Extracting
{
    public class CSharpTextExtractorSettings
    {
        public static readonly CSharpTextExtractorSettings Default = new CSharpTextExtractorSettings
        {
            TranslatorMemberName = "T",
            PluralTypeName = "Plural",
            PluralFactoryMemberName = "From",
            TextContextTypeName = "TextContext",
            TextContextFactoryMemberName = "From",
        };

        public string TranslatorMemberName { get; set; }
        public string PluralTypeName { get; set; }
        public string PluralFactoryMemberName { get; set; }
        public string TextContextTypeName { get; set; }
        public string TextContextFactoryMemberName { get; set; }
    }

    public class CSharpTextExtractor : ILocalizableTextExtractor
    {
        static int GetLine(SyntaxNode node, CancellationToken cancellationToken)
        {
            var lineSpan = node.SyntaxTree.GetMappedLineSpan(node.Span, cancellationToken);
            return lineSpan.StartLinePosition.Line + 1;
        }

        static string GetId(BaseArgumentListSyntax node)
        {
            var args = node.Arguments;
            return
                args.Count > 0 &&
                    args[0] is ArgumentSyntax arg &&
                    arg.Expression is LiteralExpressionSyntax literal &&
                    literal.Token.IsKind(SyntaxKind.StringLiteralToken) ?
                literal.Token.ValueText :
                null;
        }

        static string GetPluralId(BaseArgumentListSyntax node, CSharpTextExtractorSettings settings)
        {
            var factoryInvocation = node.Arguments
                .Skip(1)
                .Select(a =>
                    a is ArgumentSyntax arg &&
                        arg.Expression is InvocationExpressionSyntax invocation &&
                        invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                        memberAccess.Expression is IdentifierNameSyntax typeName &&
                        typeName.Identifier.ValueText == settings.PluralTypeName &&
                        memberAccess.Name is IdentifierNameSyntax memberName &&
                        memberName.Identifier.ValueText == settings.PluralFactoryMemberName ?
                    invocation :
                    null)
                .FirstOrDefault(inv => inv != null);

            if (factoryInvocation == null)
                return null;

            var args = factoryInvocation.ArgumentList.Arguments;
            return
                args.Count == 2 &&
                    args[0] is ArgumentSyntax argument &&
                    argument.Expression is LiteralExpressionSyntax literal &&
                    literal.Token.IsKind(SyntaxKind.StringLiteralToken) ?
                literal.Token.ValueText :
                null;
        }

        static string GetContextId(BaseArgumentListSyntax node, CSharpTextExtractorSettings settings)
        {
            var args = node.Arguments;
            if (!(args.Count > 1 &&
                args[args.Count - 1] is ArgumentSyntax arg &&
                    arg.Expression is InvocationExpressionSyntax factoryInvocation &&
                    factoryInvocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Expression is IdentifierNameSyntax typeName &&
                    typeName.Identifier.ValueText == settings.TextContextTypeName &&
                    memberAccess.Name is IdentifierNameSyntax memberName &&
                    memberName.Identifier.ValueText == settings.TextContextFactoryMemberName))
                return null;

            args = factoryInvocation.ArgumentList.Arguments;
            return
                args.Count == 1 &&
                    args[0] is ArgumentSyntax argument &&
                    argument.Expression is LiteralExpressionSyntax literal &&
                    literal.Token.IsKind(SyntaxKind.StringLiteralToken) ?
                literal.Token.ValueText :
                null;
        }

        static LocalizableTextInfo GetTextInfo(ElementAccessExpressionSyntax translateExpression, CSharpTextExtractorSettings settings, CancellationToken cancellationToken)
        {
            var line = GetLine(translateExpression, cancellationToken);

            var argList = translateExpression.ArgumentList;
            var id = GetId(argList);
            if (id == null)
                return new LocalizableTextInfo { Line = line };

            return new LocalizableTextInfo
            {
                ContextId = GetContextId(argList, settings),
                Line = line,
                Id = id,
                PluralId = GetPluralId(argList, settings)
            };
        }

        public CSharpTextExtractor() : this(null) { }

        public CSharpTextExtractor(CSharpTextExtractorSettings settings)
        {
            Settings = settings ?? CSharpTextExtractorSettings.Default;
        }

        protected CSharpTextExtractorSettings Settings { get; }

        protected virtual string GetCode(string content, CancellationToken cancellationToken)
        {
            return content;
        }

        protected virtual SyntaxTree ParseText(string code, CancellationToken cancellationToken)
        {
            return CSharpSyntaxTree.ParseText(code, cancellationToken: cancellationToken);
        }

        protected virtual IEnumerable<SyntaxNode> GetRootNodes(SyntaxTree syntaxTree, CancellationToken cancellationToken)
        {
            return syntaxTree.GetRoot(cancellationToken).DescendantNodes()
                .Where(md => md is MethodDeclarationSyntax || md is PropertyDeclarationSyntax);
        }

        public IEnumerable<LocalizableTextInfo> Extract(string content, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            if (string.IsNullOrEmpty(Settings.TranslatorMemberName))
                return Enumerable.Empty<LocalizableTextInfo>();

            var code = GetCode(content, cancellationToken);

            var syntaxTree = ParseText(code, cancellationToken);
            if (syntaxTree.GetDiagnostics(cancellationToken).Any(d => d.Severity >= DiagnosticSeverity.Error))
                throw new ArgumentException("Source code has errors", nameof(content));

            var root = syntaxTree.GetRoot(cancellationToken);

            return GetRootNodes(syntaxTree, cancellationToken)
                .SelectMany(n => n.DescendantNodes())
                .OfType<ElementAccessExpressionSyntax>()
                .Where(ie => (ie.Expression is IdentifierNameSyntax identifier) && identifier.Identifier.ValueText == Settings.TranslatorMemberName)
                .Select(ie => GetTextInfo(ie, Settings, cancellationToken));
        }
    }
}
