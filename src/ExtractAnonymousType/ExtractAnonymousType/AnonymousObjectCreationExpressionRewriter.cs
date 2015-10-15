using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            if (model.GetTypeInfo(node.Declaration.Type).Type.Equals(targetAnonymousType))
            {
                var declaration = node.DescendantNodes().OfType<AnonymousObjectCreationExpressionSyntax>().Single();
                //Add the name of the new type just after the "new"
                var newDeclarationText = declaration.ToString().Insert(3, " " + newTypeName);
                var newDeclaration = SyntaxFactory.ParseExpression(newDeclarationText);

                return node.ReplaceNode(declaration, newDeclaration);
            }
            else
            {
                return base.VisitLocalDeclarationStatement(node);
            }
        }
    }
}
