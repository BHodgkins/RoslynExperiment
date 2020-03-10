using System;
using System.IO;
using System.Threading.Tasks;
using ConstructionCS;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;

namespace TransformationCS
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await TransformQuickStart(args);
        }

        static async Task FormatSolution(string[] args)
        {
            // Locate and register the default instance of MSBuild installed on this machine.
            MSBuildLocator.RegisterDefaults();

            // The test solution is copied to the output directory when you build this sample.
            var workspace = MSBuildWorkspace.Create();

            var solutionPath = @"..\..\..\..\SyntaxTransformationQuickStart.sln";
            // Open the solution within the workspace.
            var originalSolution = await workspace.OpenSolutionAsync(solutionPath);

            // Declare a variable to store the intermediate solution snapshot at each step.
            var newSolution = originalSolution;

            // Note how we can't simply iterate over originalSolution.Projects or project.Documents
            // because it will return objects from the unmodified originalSolution, not from the newSolution.
            // We need to use the ProjectIds and DocumentIds (that don't change) to look up the corresponding
            // snapshots in the newSolution.
            foreach (ProjectId projectId in originalSolution.ProjectIds)
            {
                // Look up the snapshot for the original project in the latest forked solution.
                var project = newSolution.GetProject(projectId);

                foreach (DocumentId documentId in project.DocumentIds)
                {
                    // Look up the snapshot for the original document in the latest forked solution.
                    var document = newSolution.GetDocument(documentId);

                    // Get a transformed version of the document (a new solution snapshot is created
                    // under the covers to contain it - none of the existing objects are modified).
                    var newDocument = await Formatter.FormatAsync(document);

                    // Store the solution implicitly constructed in the previous step as the latest
                    // one so we can continue building it up in the next iteration.
                    newSolution = newDocument.Project.Solution;
                }
            }

            // Actually apply the accumulated changes and save them to disk. At this point
            // workspace.CurrentSolution is updated to point to the new solution.
            if (workspace.TryApplyChanges(newSolution))
            {
                Console.WriteLine("Solution updated.");
            }
            else
            {
                Console.WriteLine("Update failed!");
            }
        }

        static async Task TransformQuickStart(string[] args)
        {

            string solutionPath = @"..\..\..\..\SyntaxTransformationQuickStart.sln";

            var workspace = new AdhocWorkspace();

            var solution = SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Create(), solutionPath);

            var originalSolution = workspace.AddSolution(solution);
            
            var newSolution = originalSolution;

            foreach (ProjectId projectId in originalSolution.ProjectIds)
            {
                var newProject = newSolution.GetProject(projectId);

                foreach (var documentId in newProject.DocumentIds)
                {
                    var document = newSolution.GetDocument(documentId);

                    var tree = await document.GetSyntaxTreeAsync();
                    var model = await document.GetSemanticModelAsync();

                    var rewriter = new TypeInferenceRewriter(model);

                    var newSource = rewriter.Visit(tree.GetRoot());

                    var newDocument = document.WithText(newSource.GetText());

                    newSolution = newDocument.Project.Solution;
                }
            }

            if (workspace.TryApplyChanges(newSolution))
            {
                Console.WriteLine("Solution updated.");
            }
            else
            {
                Console.WriteLine("Update failed!");
            }
        }

        static async Task TransformTest(string[] args)
        {
            var test = CreateTestCompilation();

            foreach (SyntaxTree sourceTree in test.SyntaxTrees)
            {
                var model = test.GetSemanticModel(sourceTree);

                var rewriter = new TypeInferenceRewriter(model);

                var newSource = rewriter.Visit(sourceTree.GetRoot());

                if (newSource != sourceTree.GetRoot())
                {
                    File.WriteAllText(sourceTree.FilePath, newSource.ToFullString());
                }
            }

            await Task.CompletedTask;
        }

        private static Compilation CreateTestCompilation()
        {
            var programPath = @"..\..\..\Program.cs";
            var programText = File.ReadAllText(programPath);
            var programTree =
                           CSharpSyntaxTree.ParseText(programText)
                                           .WithFilePath(programPath);

            var rewriterPath = @"..\..\..\TypeInferenceRewriter.cs";
            var rewriterText = File.ReadAllText(rewriterPath);
            var rewriterTree =
                           CSharpSyntaxTree.ParseText(rewriterText)
                                           .WithFilePath(rewriterPath);

            SyntaxTree[] sourceTrees = { programTree, rewriterTree };

            MetadataReference mscorlib =
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            MetadataReference codeAnalysis =
                    MetadataReference.CreateFromFile(typeof(SyntaxTree).Assembly.Location);
            MetadataReference csharpCodeAnalysis =
                    MetadataReference.CreateFromFile(typeof(CSharpSyntaxTree).Assembly.Location);

            MetadataReference[] references = { mscorlib, codeAnalysis, csharpCodeAnalysis };

            return CSharpCompilation.Create("TransformationCS",
                sourceTrees,
                references,
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        }
    }
}
