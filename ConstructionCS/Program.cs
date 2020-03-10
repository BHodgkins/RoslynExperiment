using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static System.Console;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ConstructionCS
{
    class Program
    {
        private const string sampleCode =
  @"using System;
using System.Collections;
using System.Linq;
using System.Text;
 
namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello, World!"");
        }
    }
}";

        static void Main(string[] args)
        {
            NameSyntax name = IdentifierName("System");
            WriteLine($"\tCreated the identifier {name.ToString()}");
            name = QualifiedName(name, IdentifierName("Collections"));
            WriteLine(name.ToString());
            name = QualifiedName(name, IdentifierName("Generic"));
            WriteLine(name.ToString());

            var tree = CSharpSyntaxTree.ParseText(sampleCode);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var oldUsing = root.Usings[1];
            var newUsing = oldUsing.WithName(name);
            WriteLine(root.ToString());
            root = root.ReplaceNode(oldUsing, newUsing);
            WriteLine(root.ToString());
        }
    }
}
