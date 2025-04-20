// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using ConsoleApp1;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

SyntaxChecker syntaxChecker = new SyntaxChecker();


string filePath = Path.Combine(Directory.GetCurrentDirectory(), "samples", "Cipher.cs");
string fileContent = null;

// Use the first argument as the directory if provided; otherwise, default to the current directory.
string initialDirectory = args.Length > 0 ? args[0] : Environment.CurrentDirectory;

Action<string, string, string> onFileFound = (fullPath, fileName, filePath) => {
    // Console.WriteLine(fullPath);
    
    try
    {
        fileContent = File.ReadAllText(fullPath);
        Console.WriteLine($"Successfully read file from: {fullPath}");
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

    syntaxChecker.ParseContent(fileContent, fileName, fullPath);
};


Stopwatch stopwatch = Stopwatch.StartNew();

// Create an instance of FileSearcher.
FileSearcher fileSearcher = new FileSearcher(initialDirectory, onFileFound);

stopwatch.Stop();


// Execute the search and print found .cs file paths.
fileSearcher.PrintCsFiles();


syntaxChecker.PrintReport(); 
fileSearcher.PrintSearchStatistics();
Console.WriteLine($"Time taken: {stopwatch.Elapsed}");

Console.WriteLine("");

syntaxChecker.ExportToCsv();

// Access the records for custom processing
List<AlgoRecord> records = syntaxChecker.Records;

