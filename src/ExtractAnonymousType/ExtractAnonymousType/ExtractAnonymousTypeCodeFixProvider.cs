using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Formatting;
using System.Text;

namespace ExtractAnonymousType
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExtractAnonymousTypeCodeFixProvider)), Shared]
    public class ExtractAnonymousTypeCodeFixProvider : CodeFixProvider
    {
        private const string title = "Extract type from anonymous declaration";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(ExtractAnonymousTypeAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var diagnosticSpan = diagnostic.Location.SourceSpan;

                // Find the type declaration identified by the diagnostic
                // There can only be one, so Single() can be called
                var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf()
                    .OfType<LocalDeclarationStatementSyntax>().Single();

                var typeInfo = model.GetTypeInfo(declaration.Declaration.Type);

                //The enclosing class is the first parent class found in the parents of the declaration
                var containingType = declaration.Ancestors().OfType<TypeDeclarationSyntax>().Single();

                var name = this.GetNewTypeName(containingType, model);

                // One more check, just because we can...
                if (typeInfo.Type.IsAnonymousType)
                {
                    // Register a code action that will invoke the fix.
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: title,
                            createChangedDocument: c => ExtractAnonymousType(context.Document, root, model,
                                containingType, typeInfo, name, c), equivalenceKey: title),
                        diagnostic);
                }
            }
        }

        private string GetNewTypeName(TypeDeclarationSyntax containingType, SemanticModel model)
        {
            const string defaultName = "MyType";

            var @namespace = model.GetDeclaredSymbol(containingType).ContainingNamespace;
            var types = @namespace.GetMembers().OfType<ITypeSymbol>().Select(t => t.Name);

            var name = defaultName;

            int count = 1;
            while (types.Contains(name))
            {
                name = defaultName + count++;
            }

            return name;
        }

        private async Task<Document> ExtractAnonymousType(Document document, SyntaxNode documentRoot,
            SemanticModel model, SyntaxNode containingClass, TypeInfo anonymousTypeInfo, string newClassName,
            CancellationToken cancellationToken)
        {
            var rewriter = new AnonymousObjectCreationExpressionRewriter(anonymousTypeInfo.Type, model, newClassName);
            var newRoot = rewriter.Visit(documentRoot);

            var newClassSyntax = this.GetClassSyntaxFromAnonymousType(anonymousTypeInfo, newClassName);

            var containingTypeNewNode = newRoot.DescendantNodes().OfType<TypeDeclarationSyntax>()
                .Where(c => c.Identifier.Text == (containingClass as TypeDeclarationSyntax).Identifier.Text)
                .First();

            newRoot = newRoot.InsertNodesAfter(containingTypeNewNode, newClassSyntax
                .WithLeadingTrivia(SyntaxFactory.LineFeed).ChildNodes());

            return await Formatter.FormatAsync(document.WithSyntaxRoot(newRoot));
        }

        private ClassDeclarationSyntax GetClassSyntaxFromAnonymousType(TypeInfo anonymousTypeInfo, string newClassName)
        {
            var properties = anonymousTypeInfo.Type.GetMembers()
                .Where(s => s.Kind == SymbolKind.Property)
                .Select(s => s as IPropertySymbol)
                .Select(s => new { Name = s.MetadataName, Type = s.Type.ToDisplayString() });

            var members = new List<MemberDeclarationSyntax>();

            // This monster was generated with https://github.com/KirillOsenkov/RoslynQuoter
            foreach (var p in properties)
            {
                members.Add(
                    SyntaxFactory.PropertyDeclaration(
                    SyntaxFactory.IdentifierName(
                        SyntaxFactory.Identifier(
                            SyntaxFactory.TriviaList(),
                            p.Type,
                            SyntaxFactory.TriviaList(
                                SyntaxFactory.Space))),
                        SyntaxFactory.Identifier(
                            SyntaxFactory.TriviaList(),
                            p.Name,
                            SyntaxFactory.TriviaList(
                                SyntaxFactory.Space)))
                    .WithModifiers(
                        SyntaxFactory.TokenList(
                            SyntaxFactory.Token(
                                SyntaxFactory.TriviaList(
                                    SyntaxFactory.Whitespace(
                                        @"    ")),
                                SyntaxKind.PublicKeyword,
                                SyntaxFactory.TriviaList(
                                    SyntaxFactory.Space))))
                    .WithAccessorList(
                        SyntaxFactory.AccessorList(
                            SyntaxFactory.List<AccessorDeclarationSyntax>(
                                new AccessorDeclarationSyntax[]{
                                    SyntaxFactory.AccessorDeclaration(
                                        SyntaxKind.GetAccessorDeclaration)
                                    .WithKeyword(
                                        SyntaxFactory.Token(
                                            SyntaxKind.GetKeyword))
                                    .WithSemicolonToken(
                                        SyntaxFactory.Token(
                                            SyntaxFactory.TriviaList(),
                                            SyntaxKind.SemicolonToken,
                                            SyntaxFactory.TriviaList(
                                                SyntaxFactory.Space))),
                                    SyntaxFactory.AccessorDeclaration(
                                        SyntaxKind.SetAccessorDeclaration)
                                    .WithKeyword(
                                        SyntaxFactory.Token(
                                            SyntaxKind.SetKeyword))
                                    .WithSemicolonToken(
                                        SyntaxFactory.Token(
                                            SyntaxFactory.TriviaList(),
                                            SyntaxKind.SemicolonToken,
                                            SyntaxFactory.TriviaList(
                                                SyntaxFactory.Space)))}))
                        .WithOpenBraceToken(
                            SyntaxFactory.Token(
                                SyntaxFactory.TriviaList(),
                                SyntaxKind.OpenBraceToken,
                                SyntaxFactory.TriviaList(
                                    SyntaxFactory.Space)))
                        .WithCloseBraceToken(
                            SyntaxFactory.Token(
                                SyntaxFactory.TriviaList(),
                                SyntaxKind.CloseBraceToken,
                                SyntaxFactory.TriviaList(
                                    SyntaxFactory.LineFeed)))));
            }

            return SyntaxFactory.ClassDeclaration(
                SyntaxFactory.Identifier(
                    SyntaxFactory.TriviaList(),
                    newClassName,
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.LineFeed)))
                .WithKeyword(
                    SyntaxFactory.Token(
                        SyntaxFactory.TriviaList(),
                        SyntaxKind.ClassKeyword,
                        SyntaxFactory.TriviaList(
                            SyntaxFactory.Space)))
                .WithOpenBraceToken(
                    SyntaxFactory.Token(
                        SyntaxFactory.TriviaList(),
                        SyntaxKind.OpenBraceToken,
                        SyntaxFactory.TriviaList(
                            SyntaxFactory.LineFeed)))
                .WithMembers(SyntaxFactory.List(members))
                .WithCloseBraceToken(
                    SyntaxFactory.Token(
                        SyntaxKind.CloseBraceToken));
        }
    }
}