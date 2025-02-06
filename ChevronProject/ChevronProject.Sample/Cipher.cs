using System;
using System.Security.Cryptography;
using System.Text;

public class CryptoExample
{
    public static void Main(string[] args)
    {
        // Generate a Symmetric Key (AES)
        using (Aes aes = Aes.Create())
        {
            Console.WriteLine("AES Key:");
            Console.WriteLine(Convert.ToBase64String(aes.Key));
            Console.WriteLine("AES IV:");
            Console.WriteLine(Convert.ToBase64String(aes.IV));

            string plaintext = "This is a secret message.";

            // Encrypt the plaintext
            byte[] ciphertext = EncryptString(plaintext, aes.Key, aes.IV);
            Console.WriteLine("\nCiphertext:");
            Console.WriteLine(Convert.ToBase64String(ciphertext));


            // Decrypt the ciphertext
            string decryptedText = DecryptString(ciphertext, aes.Key, aes.IV);
            Console.WriteLine("\nDecrypted Text:");
            Console.WriteLine(decryptedText);


            //Example of Hashing (SHA256)
            string dataToHash = "Some data to hash";
            byte[] hash = ComputeSHA256Hash(dataToHash);
            Console.WriteLine("\nSHA256 Hash:");
            Console.WriteLine(Convert.ToBase64String(hash));


            // RSA Key Pair Generation and Signing/Verification
            using (RSA rsa = RSA.Create())
            {
                // Get public and private keys
                string publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                string privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

                Console.WriteLine("\nRSA Public Key:");
                Console.WriteLine(publicKey);
                Console.WriteLine("\nRSA Private Key:");
                Console.WriteLine(privateKey);

                // Sign data (using private key)
                byte[] signature = rsa.SignData(Encoding.UTF8.GetBytes("Data to sign"), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                Console.WriteLine("\nRSA Signature:");
                Console.WriteLine(Convert.ToBase64String(signature));

                // Verify the signature (using public key)
                bool verified = rsa.VerifyData(Encoding.UTF8.GetBytes("Data to sign"), signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                Console.WriteLine("\nRSA Signature Verified:");
                Console.WriteLine(verified);
            }

        }
    }

    // AES Encryption
    static byte[] EncryptString(string plaintext, byte[] key, byte[] iv)
    {
        using (Aes aes = Aes.Create())
        using (ICryptoTransform encryptor = aes.CreateEncryptor(key, iv))
        using (MemoryStream msEncrypt = new MemoryStream())
        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        {
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            csEncrypt.Write(plaintextBytes, 0, plaintextBytes.Length);
            csEncrypt.FlushFinalBlock();
            return msEncrypt.ToArray();
        }
    }

    // AES Decryption
    static string DecryptString(byte[] ciphertext, byte[] key, byte[] iv)
    {
        using (Aes aes = Aes.Create())
        using (ICryptoTransform decryptor = aes.CreateDecryptor(key, iv))
        using (MemoryStream msDecrypt = new MemoryStream(ciphertext))
        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
        {
            return srDecrypt.ReadToEnd();
        }
    }

    // SHA256 Hashing
    static byte[] ComputeSHA256Hash(string data)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        }
    }

}