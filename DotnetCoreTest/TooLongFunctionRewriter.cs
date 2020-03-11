using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace DotnetCoreTest
{
    public class TooLongFunctionRewriter : CSharpSyntaxRewriter
    {
        //public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        //{
        //    if (node.ParameterList.Parameters.Count == 0)
        //    {
        //        return node;
        //    }

        //    var declarationSpanLength = node.Body.SpanStart - node.SpanStart;

        //    if (declarationSpanLength <= 120)
        //    {
        //        return node;
        //    }

        //    var parameters = node.ParameterList.DescendantNodes().OfType<ParameterSyntax>();

        //    foreach(var param in parameters)
        //    {
        //        param.track
        //    }

        //    Console.WriteLine(node.Identifier);

        //    return node;
        //}

        public override SyntaxNode VisitParameter(ParameterSyntax node)
        {
            var parentMethod = (MethodDeclarationSyntax) node.Ancestors().FirstOrDefault(parent => parent.IsKind(SyntaxKind.MethodDeclaration));

            if (parentMethod == null)
            {
                return node;
            }

            if (parentMethod.ParameterList.Parameters.Count == 0)
            {
                return node;
            }

            var declarationSpanLength = parentMethod.Body.SpanStart - parentMethod.SpanStart;

            if (declarationSpanLength <= 120)
            {
                return node;
            }
            Console.WriteLine($"{parentMethod.Identifier}: {node.Identifier}");

            var parentIndentation = parentMethod.GetLeadingTrivia().Last(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia));

            var newNode = node
                .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

            newNode = newNode.NormalizeWhitespace().WithLeadingTrivia(SyntaxFactory.Whitespace($"{parentIndentation}    "));

            return node.ReplaceNode(node, newNode);
        }
    }
}
