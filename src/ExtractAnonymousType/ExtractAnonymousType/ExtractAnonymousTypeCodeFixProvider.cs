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

                //The enclosing class is the first parent class found in the parents of the declaration
                var containingClass = declaration.Ancestors().OfType<ClassDeclarationSyntax>().Single();

                var typeInfo = model.GetTypeInfo(declaration.Declaration.Type);

                // One more check, just because we can...
                if (typeInfo.Type.IsAnonymousType)
                {
                    // Register a code action that will invoke the fix.
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: title,
                            createChangedDocument: c => ExtractAnonymousType(context.Document, root, model,
                                containingClass, typeInfo, c), equivalenceKey: title),
                        diagnostic);
                }
            }
        }

        private async Task<Document> ExtractAnonymousType(Document document, SyntaxNode documentRoot,
            SemanticModel model, SyntaxNode containingClass, TypeInfo typeInfo, CancellationToken cancellationToken)
        {
            var properties = typeInfo.Type.GetMembers()
                .Where(s => s.Kind == SymbolKind.Property)
                .Select(s => s as IPropertySymbol)
                .Select(s => new { Name = s.MetadataName, Type = s.Type.ToDisplayString() });

            var name = "Anon1";

            var rewriter = new AnonymousObjectCreationExpressionRewriter(typeInfo.Type, model, name);
            var newRoot = rewriter.Visit(documentRoot);

            //SyntaxFactory.ClassDeclaration(name)
            //    .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>(new[] {
            //        SyntaxFactory.PropertyDeclaration()
            //    }));
            var sb = new StringBuilder().AppendLine("class " + name);
            sb.AppendLine("{");
            foreach (var p in properties)
            {
                sb.AppendLine($"    public {p.Type} {p.Name} {{ get; set; }}");
            }
            sb.AppendLine("}");

            var newType = await CSharpSyntaxTree.ParseText(sb.ToString()).GetRootAsync();
            var containingClassNewNode = newRoot.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .Where(c => c.Identifier.Text == (containingClass as ClassDeclarationSyntax).Identifier.Text)
                .First();
            
            newRoot = newRoot.InsertNodesAfter(containingClassNewNode, newType
                .WithLeadingTrivia(SyntaxFactory.LineFeed).ChildNodes());

            return await Formatter.FormatAsync(document.WithSyntaxRoot(newRoot));
        }
    }
}