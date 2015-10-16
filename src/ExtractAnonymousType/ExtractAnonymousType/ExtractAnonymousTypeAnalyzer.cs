using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

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

        private const string Category = "Design";

        private static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info,
                isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest =>
            ImmutableArray.Create(SyntaxKind.AnonymousObjectCreationExpression);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.AnonymousObjectCreationExpression);
        }

        private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
        {
            var creationExpression = (AnonymousObjectCreationExpressionSyntax)context.Node;

            var diagnostic = Diagnostic.Create(Rule, creationExpression.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
