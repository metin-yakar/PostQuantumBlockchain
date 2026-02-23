using System;
using BlockchainEngine;

namespace PostQuantumBlockchain;

/// <summary>
/// This is a simple example application demonstrating how to use the BlockchainEngine.
/// It uses a fixed dummy message and generates a fresh key pair at runtime to showcase the cryptographic flow.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("==================================================");
        Console.WriteLine("    BlockchainEngine Demonstration App            ");
        Console.WriteLine("==================================================");
        Console.WriteLine();

        // 1. Generate a Post-Quantum Secure Key Pair
        Console.WriteLine("[STEP 1] Generating Key Pair...");
        var generator = new KeyGenerator();
        var keys = generator.GenerateKeyPair();
        
        Console.WriteLine($"Private Key : {keys.PrivateKey}");
        Console.WriteLine($"Public Key  : {keys.PublicKey}");
        Console.WriteLine();

        // 2. Define a fixed message to sign
        string message = "TransactionData: Send 50 Coins from Wallet A to Wallet B";
        Console.WriteLine($"[STEP 2] Message to Sign: '{message}'");
        Console.WriteLine();

        // 3. Sign the message using the engine
        Console.WriteLine("[STEP 3] Signing the Message...");
        string signature = "";
        try
        {
            signature = SigningEngine.Sign(message, keys.PrivateKey);
            Console.WriteLine("Generated Signature (Base62 Obfuscated):");
            Console.WriteLine(signature);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during signing: {ex.Message}");
            Console.WriteLine("\n[3] Network Verification Phase");
        Console.WriteLine("------------------------------------------");
        Console.WriteLine("A network node has intercepted the signature. Checking cryptographic integrity...");
        
        // P2P or Network Validations strictly resolve mathematically WITHOUT the private key natively inside BlockchainEngine
        string verificationResult = SigningEngine.Verify(message, signature, keys.PublicKey);
            
            if (verificationResult.StartsWith("1-"))
            {
                Console.WriteLine("SUCCESS - Signature is Valid.");
                Console.WriteLine($"Raw Output: {verificationResult}");
            }
            else
            {
                Console.WriteLine("FAILED - Signature is Invalid.");
                Console.WriteLine($"Raw Output: {verificationResult}");
            }
            return;
        }
        Console.WriteLine();

        // 4. Verify the signature
        Console.WriteLine("[STEP 4] Verifying the Signature...");
        try
        {
            // The verification returns strictly 1-[PublicKey]-[Timestamp] if successful, 0 if it fails.
            string verificationResult = SigningEngine.Verify(message, signature, keys.PublicKey);
            
            if (verificationResult.StartsWith("1-"))
            {
                Console.WriteLine("SUCCESS - Signature is Valid.");
                Console.WriteLine($"Raw Output: {verificationResult}");
            }
            else
            {
                Console.WriteLine("FAILED - Signature is Invalid.");
                Console.WriteLine($"Raw Output: {verificationResult}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during verification: {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("==================================================");
        Console.WriteLine("    Demonstration Complete                        ");
        Console.WriteLine("==================================================");
    }
}
