using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConsoleApp1;

public class SyntaxChecker
{
    private Dictionary<string, HashSet<string>> classMethodMap;
    private List<AlgoRecord> records;
    private string currentFileName;
    private string currentFilePath;
    
    public SyntaxChecker()
    {
        classMethodMap = new Dictionary<string, HashSet<string>>();
        records = new List<AlgoRecord>();
        LoadMethodsFromJson();
    }

    /// <summary>
    /// Gets the list of algorithm usage records found during parsing
    /// </summary>
    public List<AlgoRecord> Records => records;

    /// <summary>
    /// Loads the cryptographic classes and their methods from the JSON file
    /// </summary>
    private void LoadMethodsFromJson()
    {
        try
        {
            // Get the directory where the assembly is located
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            string assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            
            // Navigate to project root - adjust as needed based on your project structure
            string projectDirectory = Path.GetFullPath(Path.Combine(assemblyDirectory, @"../../../"));
            
            // Path to your JSON file in the project root
            string filePath = Path.Combine(projectDirectory, "input.json");
            
            // Read the JSON file content
            string jsonContent = File.ReadAllText(filePath);
            
            // Deserialize JSON into Dictionary<string, string[]>
            var tempDict = JsonSerializer.Deserialize<Dictionary<string, string[]>>(jsonContent);
            
            // Convert arrays to HashSets
            foreach (var kvp in tempDict)
            {
                classMethodMap[kvp.Key] = new HashSet<string>(kvp.Value);
            }
            
            Console.WriteLine($"Successfully loaded {classMethodMap.Count} cryptographic classes from JSON");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading methods from JSON: {ex.Message}");
            classMethodMap = new Dictionary<string, HashSet<string>>();
        }
    }

    /// <summary>
    /// Parses C# file content and identifies usage of cryptographic classes and methods
    /// </summary>
    /// <param name="fileContent">The content of the C# file to analyze</param>
    /// <param name="fileName">The name of the file being analyzed</param>
    /// <param name="filePath">The path to the file being analyzed</param>
    public void ParseContent(string fileContent, string fileName, string filePath)
    {
        if (string.IsNullOrWhiteSpace(fileContent))
            return;

        this.currentFileName = fileName;
        this.currentFilePath = filePath;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(fileContent);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

        foreach (var node in root.DescendantNodes())
        {
            // Look for method invocations
            if (node is InvocationExpressionSyntax invocation)
            {
                ProcessInvocation(invocation);
            }
            // Look for object creation
            else if (node is ObjectCreationExpressionSyntax objectCreation)
            {
                ProcessObjectCreation(objectCreation);
            }
        }
    }

    /// <summary>
    /// Processes an invocation expression to identify cryptographic method usage
    /// </summary>
    private void ProcessInvocation(InvocationExpressionSyntax invocation)
    {
        // Handle member access (Class.Method())
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var methodName = memberAccess.Name.Identifier.Text;
            
            // Static method call (Class.Method())
            if (memberAccess.Expression is IdentifierNameSyntax identifierName)
            {
                string className = identifierName.Identifier.Text;
                CheckAndAddRecord(className, methodName, invocation);
            }
            // Qualified name like System.Security.Cryptography.Class.Method()
            else if (memberAccess.Expression is QualifiedNameSyntax qualifiedName)
            {
                string qualifiedClassName = ExtractClassName(qualifiedName);
                CheckAndAddRecord(qualifiedClassName, methodName, invocation);
            }
            // // Instance method call on a variable (var.Method())
            // else
            // {
            //     // For instance method calls, we need semantic model to determine the type
            //     // For now, we'll just check if the method is in our known cryptographic methods
            //     foreach (var classEntry in classMethodMap)
            //     {
            //         if (classEntry.Value.Contains(methodName))
            //         {
            //             int lineNumber = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            //             records.Add(new AlgoRecord(
            //                 "Unknown", // Can't determine class without semantic model
            //                 methodName,
            //                 currentFileName,
            //                 currentFilePath,
            //                 lineNumber
            //             ));
            //             break;
            //         }
            //     }
            // }
        }
    }

    /// <summary>
    /// Processes an object creation expression to identify cryptographic class instantiation
    /// </summary>
    private void ProcessObjectCreation(ObjectCreationExpressionSyntax objectCreation)
    {
        string className = objectCreation.Type.ToString();
        
        // Check if the class being instantiated is in our map
        if (classMethodMap.ContainsKey(className))
        {
            int lineNumber = objectCreation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            records.Add(new AlgoRecord(
                className,
                "constructor",
                currentFileName,
                currentFilePath,
                lineNumber
            ));
        }
        
        // Handle shorthand class name (without namespace)
        foreach (var kvp in classMethodMap)
        {
            string shortClassName = kvp.Key.Split('.').Last();
            if (className == shortClassName)
            {
                int lineNumber = objectCreation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                records.Add(new AlgoRecord(
                    kvp.Key,
                    "constructor",
                    currentFileName,
                    currentFilePath,
                    lineNumber
                ));
                break;
            }
        }
    }

    /// <summary>
    /// Checks if the class and method are in our tracking list and adds a record if found
    /// </summary>
    private void CheckAndAddRecord(string className, string methodName, SyntaxNode node)
    {
        // Check fully qualified class name
        if (classMethodMap.TryGetValue(className, out var methods) && methods.Contains(methodName))
        {
            int lineNumber = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            records.Add(new AlgoRecord(
                className,
                methodName,
                currentFileName,
                currentFilePath,
                lineNumber
            ));
            return;
        }
        
        // Check shorthand class name (without namespace)
        foreach (var kvp in classMethodMap)
        {
            string shortClassName = kvp.Key.Split('.').Last();
            if (className == shortClassName && kvp.Value.Contains(methodName))
            {
                int lineNumber = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                records.Add(new AlgoRecord(
                    kvp.Key,
                    methodName,
                    currentFileName,
                    currentFilePath,
                    lineNumber
                ));
                break;
            }
        }
    }

    /// <summary>
    /// Extracts the class name from a qualified name syntax
    /// </summary>
    private string ExtractClassName(QualifiedNameSyntax qualifiedName)
    {
        // For names like System.Security.Cryptography.RSA, get "RSA"
        if (qualifiedName.Right is IdentifierNameSyntax right)
        {
            string className = right.Identifier.Text;
            
            // Check if this specific class name exists in our map
            if (classMethodMap.ContainsKey(className))
                return className;
            
            // Return the full qualified name for checking with namespace
            return qualifiedName.ToString();
        }
        return qualifiedName.ToString();
    }

    /// <summary>
    /// Generates a report of all crypto algorithm usages found during parsing
    /// </summary>
    public void PrintReport()
    {
        if (records.Count == 0)
        {
            Console.WriteLine("No cryptographic algorithm usage detected.");
            return;
        }

        Console.WriteLine("===== Cryptographic Algorithm Usage Report =====");
        
        // Group by class name and method
        var grouped = records
            .GroupBy(r => new { r.Name, r.Method })
            .OrderBy(g => g.Key.Name)
            .ThenBy(g => g.Key.Method);
            
        foreach (var group in grouped)
        {
            Console.WriteLine($"{group.Key.Name}.{group.Key.Method}: {group.Count()} occurrences");
            
            foreach (var record in group)
            {
                Console.WriteLine($"  - {record.FilePath}:{record.LineNumber}");
            }
        }
        
        Console.WriteLine($"\nTotal instances found: {records.Count}");
    }
    
    // Method to export records to CSV
    public void ExportToCsv(string filePathIn = "output.csv")
    {
        if (records == null || records.Count == 0)
        {
            Console.WriteLine("No records to export.");
            return;
        }
        
        try
        {
            // Create StringBuilder for building CSV content
            StringBuilder csvContent = new StringBuilder();
            
            // Add header row
            csvContent.AppendLine("Name,Method,FileName,FilePath,LineNumber");
            
            // Add data rows
            foreach (var record in records)
            {
                // Escape fields that might contain commas
                string name = EscapeCsvField(record.Name);
                string method = EscapeCsvField(record.Method);
                string fileName = EscapeCsvField(record.FileName);
                string filePath = EscapeCsvField(record.FilePath);
                
                csvContent.AppendLine($"{name},{method},{fileName},{filePath},{record.LineNumber}");
            }
            
            // Write to file
            File.WriteAllText(filePathIn, csvContent.ToString());
            
            Console.WriteLine($"CSV file successfully created at: {Path.GetFullPath(filePathIn)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exporting to CSV: {ex.Message}");
        }
    }

    // Helper method to escape CSV fields
    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;
            
        // If the field contains commas, quotes, or newlines, wrap it in quotes
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
        {
            // Replace any double quotes with two double quotes
            field = field.Replace("\"", "\"\"");
            return $"\"{field}\"";
        }
        
        return field;
    }
}
