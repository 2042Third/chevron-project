using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConsoleApp1;

public class SyntaxChecker
{
    
   
    private Dictionary<string, int> createCounts = new Dictionary<string, int>();
    private HashSet<string> algo_set = new HashSet<string>{"Aes", "RSA"};

    public SyntaxChecker()
    {
        foreach (var algo in algo_set)
        {
            createCounts[algo] = 0;
            createCounts["System.Security.Cryptography."+algo] = 0;
        }
    }
    
    public void UpdateCreateCounts(ExpressionSyntax expression)
    {
        // Check for simple identifier 'Aes'
        if (expression is IdentifierNameSyntax identifierName )
        {
            if (algo_set.Contains(identifierName.Identifier.Text))
            {
                createCounts[identifierName.Identifier.Text]++;
            }
        }
        // Check for qualified name like 'System.Security.Cryptography.Aes'
        else if (expression is QualifiedNameSyntax qualifiedName)
        {
            if (algo_set.Contains(qualifiedName.ToString()))
            {
                createCounts[qualifiedName.ToString()]++;
            }
        }
        // Check for generic name like 'Aes<T>' (though Aes itself is not generic, for broader type checking)
        else if (expression is GenericNameSyntax genericName)
        {
            if (algo_set.Contains(genericName.Identifier.Text))
            {
                createCounts[genericName.Identifier.Text]++;
            }
        }
    }
    
    public void PrintCreateCounts()
    {
        foreach (var algo in algo_set)
        {
            Console.WriteLine($"Number of {algo} instances created: {createCounts[algo]}");
            Console.WriteLine($"Number of System.Security.Cryptography.{algo} instances created: {createCounts["System.Security.Cryptography."+algo]}");
        }
    }
    
    public bool IsAesType(ExpressionSyntax expression)
    {
        // Check for simple identifier 'Aes'
        if (expression is IdentifierNameSyntax identifierName)
        {
            return identifierName.Identifier.Text == "Aes";
        }
        // Check for qualified name like 'System.Security.Cryptography.Aes'
        else if (expression is QualifiedNameSyntax qualifiedName)
        {
            return qualifiedName.ToString() == "System.Security.Cryptography.Aes"; // Simplest check, might need refinement
        }
        // Check for generic name like 'Aes<T>' (though Aes itself is not generic, for broader type checking)
        else if (expression is GenericNameSyntax genericName)
        {
            return genericName.Identifier.Text == "Aes";
        }
        return false; // Not an Aes type (or a type we can easily identify as Aes in this simple check)
    }
}