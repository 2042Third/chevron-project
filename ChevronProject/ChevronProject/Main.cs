using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ChevronProject
{
    class Program
    {
        static void Main(string[] args)
        {
            // Define the example code to be analyzed
            string sourceCode = @"
                namespace MyCompany
                {
                    class MyClass
                    {
                    }
                }";

            // Parse the source code into a SyntaxTree
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

            // Create a compilation with the SyntaxTree and necessary references
            var compilation = CSharpCompilation.Create("ChevronProject")
                .AddSyntaxTrees(syntaxTree)
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                               MetadataReference.CreateFromFile(typeof(Console).Assembly.Location));

            // Instantiate your SampleSyntaxAnalyzer
            var analyzer = new SampleSyntaxAnalyzer();
            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);

            // Create a CompilationWithAnalyzers instance
            var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);

            // Run the analyzer on the code and retrieve diagnostics
            var diagnostics = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result;

            // Output diagnostic messages
            foreach (var diagnostic in diagnostics)
            {
                Console.WriteLine($"Diagnostic: {diagnostic.Id}, Message: {diagnostic.GetMessage()}");
            }

            Console.WriteLine("Analysis complete.");
        }
    }
}