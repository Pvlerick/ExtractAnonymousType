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
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

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
                // There can only be one, so First() can be called
                var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf()
                    .OfType<LocalDeclarationStatementSyntax>().First();

                var typeInfo = model.GetTypeInfo(declaration.Declaration.Type);

                // One more check, just because we can...
                if (typeInfo.Type.IsAnonymousType)
                {
                    // Register a code action that will invoke the fix.
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: title,
                            createChangedSolution: c => ExtractAnonymousType(context.Document, model, c),
                            equivalenceKey: title),
                        diagnostic);
                }

            }
        }

        private async Task<Solution> ExtractAnonymousType(Document document, SemanticModel model,
            CancellationToken cancellationToken)
        {
            //var def = typeInfo.Type.OriginalDefinition;

            //var symbol = model.Compilation.GetSymbolsWithName(name => name == typeInfo.Type.Name);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            return null;
            //// Compute new uppercase name.
            //var identifierToken = typeDecl.Identifier;
            //var newName = identifierToken.Text.ToUpperInvariant();

            //// Get the symbol representing the type to be renamed.
            //var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            //var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

            //// Produce a new solution that has all references to that type renamed, including the declaration.
            //var originalSolution = document.Project.Solution;
            //var optionSet = originalSolution.Workspace.Options;
            //var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            //// Return the new solution with the now-uppercase type name.
            //return newSolution;
        }
    }
}