using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ConsoleApp1;

public class SyntaxChecker
{
    
   
    private Dictionary<string, int> createCounts = new Dictionary<string, int>();
    private HashSet<string> algo_set = new HashSet<string>{"Aes", "RSA"};
    private HashSet<string> algo_methods = new HashSet<string> {"Clear", "Create", "Decrypt", "DecryptValue", "Encrypt", "EncryptValue", "ExportParameters", 
        "ExportRSAPrivateKey", "ExportRSAPrivateKeyPem", "ExportRSAPublicKey", "ExportRSAPublicKeyPem", 
        "FromXmlString", "GetMaxOutputSize", "HashData", "ImportEncryptedPkcs8PrivateKey", 
        "ImportFromEncryptedPem", "ImportFromPem", "ImportParameters", "ImportPkcs8PrivateKey", 
        "ImportRSAPrivateKey", "ImportRSAPublicKey", "ImportSubjectPublicKeyInfo", "SignData", 
        "SignHash", "ToXmlString", "TryDecrypt", "TryEncrypt", "TryExportEncryptedPkcs8PrivateKey", 
        "TryExportPkcs8PrivateKey", "TryExportRSAPrivateKey", "TryExportRSAPrivateKeyPem", 
        "TryExportRSAPublicKey", "TryExportRSAPublicKeyPem", "TryExportSubjectPublicKeyInfo", 
        "TryHashData", "TrySignData", "TrySignHash", "VerifyData", "VerifyHash"};

    public SyntaxChecker()
    {
        foreach (var algo in algo_set)
        {
            createCounts[algo] = 0;
            createCounts["System.Security.Cryptography."+algo] = 0;
        }
    }

    public void UpdateMethodCounts()
    {
        
    }

    public void UpdateClassCounts(ExpressionSyntax expression)
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
    
    public void PrintCounts()
    {
        foreach (var algo in algo_set)
        {
            Console.WriteLine($"Number of {algo} instances created: {createCounts[algo]}");
            Console.WriteLine($"Number of System.Security.Cryptography.{algo} instances created: {createCounts["System.Security.Cryptography."+algo]}");
        }
    }
    
}