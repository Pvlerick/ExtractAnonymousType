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
                            createChangedDocument: c => ExtractAnonymousType(context.Document, root, containingClass,
                            typeInfo, c), equivalenceKey: title),
                        diagnostic);
                }
            }
        }

        private async Task<Document> ExtractAnonymousType(Document document, SyntaxNode documentRoot,
            SyntaxNode containingClass, TypeInfo typeInfo, CancellationToken cancellationToken)
        {
            var properties = typeInfo.Type.GetMembers()
                .Where(s => s.Kind == SymbolKind.Property)
                .Select(s => new { Name = s.MetadataName, Type = s.GetType() });

            var newType = await CSharpSyntaxTree.ParseText(@"class Anon1 { public string P { get; set; } }")
                .GetRootAsync();

            var newRoot = documentRoot.InsertNodesAfter(containingClass, newType.ChildNodes());

            return await Formatter.FormatAsync(document.WithSyntaxRoot(newRoot));
        }
    }
}