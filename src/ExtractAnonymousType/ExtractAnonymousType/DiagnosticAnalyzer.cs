using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExtractAnonymousType
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExtractAnonymousTypeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ExtractAnonymousType";

        private static readonly LocalizableString Title =
            new LocalizableResourceString(nameof(Resources.AnalyzerTitle),
                Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat =
            new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat),
                Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description =
            new LocalizableResourceString(nameof(Resources.AnalyzerDescription),
                Resources.ResourceManager, typeof(Resources));

        //TODO Change Category
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info,
                isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest =>
            ImmutableArray.Create(SyntaxKind.LocalDeclarationStatement);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.LocalDeclarationStatement);
        }

        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            var localDeclaration = (LocalDeclarationStatementSyntax)context.Node;

            var typeKeyword = localDeclaration.Declaration.Type;
            var variablesCount = localDeclaration.Declaration.Variables.Count;

            //Anonymous types are always declared with the var keyword and only one can be declared at the time
            if (typeKeyword.IsVar && variablesCount == 1)
            {
                var variable = localDeclaration.Declaration.Variables[0];

                var model = context.SemanticModel;
                var variableSymbol = (ILocalSymbol)model.GetDeclaredSymbol(variable);

                var type = variableSymbol.Type;

                if (type.IsAnonymousType)
                {
                    var diagnostic = Diagnostic.Create(Rule, variableSymbol.Locations[0], variableSymbol.Name);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
