// See https://aka.ms/new-console-template for more information

using ConsoleApp1;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

string filePath = Path.Combine(Directory.GetCurrentDirectory(), "samples", "Cipher.cs");
string fileContent = null;

SyntaxChecker syntaxChecker = new SyntaxChecker();

try
{
    fileContent = File.ReadAllText(filePath);
    Console.WriteLine($"Successfully read file from: {filePath}");
    // You can now use the 'fileContent' string
    // For example, print the first few lines:
    // string[] lines = fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
    // for (int i = 0; i < Math.Min(5, lines.Length); i++)
    // {
    //     Console.WriteLine(lines[i]);
    // }
}
catch (FileNotFoundException)
{
    Console.WriteLine($"Error: File not found at path: {filePath}");
    Console.WriteLine("Please make sure the 'samples' directory and 'Cypher.cs' file exist in the current directory.");
}
catch (DirectoryNotFoundException)
{
    Console.WriteLine($"Error: Directory not found at path: {Path.GetDirectoryName(filePath)}");
    Console.WriteLine("Please make sure the 'samples' directory exists in the current directory.");
}
catch (IOException e)
{
    Console.WriteLine($"Error reading file: {e.Message}");
    Console.WriteLine($"Path: {filePath}");
}
catch (Exception e)
{
    Console.WriteLine($"An unexpected error occurred: {e.Message}");
    Console.WriteLine($"Path: {filePath}");
}

if (fileContent != null)
{
    SyntaxTree tree = CSharpSyntaxTree.ParseText(fileContent);
    CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

    foreach (var node in root.DescendantNodes())
    {
        if (node is InvocationExpressionSyntax invocation)
        {
            // Check for method calls like Class.Create()
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                // Check if the method name is "Create"
                if (memberAccess.Name.Identifier.Text == "Create")
                {
                    syntaxChecker.UpdateCreateCounts(memberAccess.Expression);
                    // // Check if the type before ".Create" is Aes
                    // if (syntaxChecker.IsAesType(memberAccess.Expression))
                    // {
                    //     Console.WriteLine($"Aes.Create() Call Detected: Full Node - {invocation}");
                    // }
                }
            }
        }
    }
}

syntaxChecker.PrintCreateCounts();

// CryptoExample.Run();

