using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace DotnetCoreTest
{
    public class LocalDeclarationPerLineRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            var semiColonFinalTrivia = node.SemicolonToken.TrailingTrivia.ToSyntaxTriviaList().Last();

            
            if (semiColonFinalTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                Console.WriteLine(node);
                var newToken = node.SemicolonToken.WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);
                

                return node.ReplaceToken(node.SemicolonToken, newToken);
            }


            return node;
        }
    }
}
