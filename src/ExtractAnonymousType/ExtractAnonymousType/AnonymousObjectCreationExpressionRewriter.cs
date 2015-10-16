using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExtractAnonymousType
{
    internal class AnonymousObjectCreationExpressionRewriter : CSharpSyntaxRewriter
    {
        private ITypeSymbol targetAnonymousType;
        private SemanticModel model;
        private string newTypeName;

        public AnonymousObjectCreationExpressionRewriter(ITypeSymbol targetAnonymousType, SemanticModel model, string newTypeName)
        {
            this.targetAnonymousType = targetAnonymousType;
            this.model = model;
            this.newTypeName = newTypeName;
        }

        public override SyntaxNode VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
        {
            if (model.GetTypeInfo(node).Type.Equals(targetAnonymousType))
            {
                var newDeclarationText = node.ToString().Insert(3, " " + newTypeName);
                var newDeclaration = SyntaxFactory.ParseExpression(newDeclarationText);

                //Cannot use type inference here otherwise a cast from ObjectCreationExpressionSyntax to
                // AnonymousObjectCreationExpressionSyntax inside ReplaceNode will fail
                return node.ReplaceNode<SyntaxNode>(node, newDeclaration);
            }
            else
            {
                return base.VisitAnonymousObjectCreationExpression(node);
            }
        }
    }
}
