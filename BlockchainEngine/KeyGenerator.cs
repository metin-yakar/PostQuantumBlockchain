using System;
using System.Security.Cryptography;

namespace BlockchainEngine;

public class KeyGenerator
{
    private const int PublicKeyMinLength = 18;
    private const int PublicKeyMaxLength = 24;
    
    public (string PrivateKey, string PublicKey) GenerateKeyPair()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        
        // Export the true private key (includes public curve parameters)
        byte[] privateKeyBytes = ecdsa.ExportECPrivateKey();
        string privateKeyBase62 = CustomEncoder.Encode(privateKeyBytes);
        
        // Export the true public key to derive the short "Address" (18-24 chars)
        byte[] truePublicKeyBytes = ecdsa.ExportSubjectPublicKeyInfo();
        string shortAddress = DeriveAddress(truePublicKeyBytes);
        
        return (privateKeyBase62, shortAddress);
    }
    
    /// <summary>
    /// Hashes the true public key to create a deterministic, compressed address (18-24 chars).
    /// </summary>
    public static string DeriveAddress(byte[] truePublicKeyBytes)
    {
        int addressLength = DetermineAddressLength(truePublicKeyBytes);
        byte[] lengthMappingBytes = BitConverter.GetBytes(addressLength);
        byte[] combinedForHash = new byte[lengthMappingBytes.Length + truePublicKeyBytes.Length];
        
        Array.Copy(lengthMappingBytes, 0, combinedForHash, 0, lengthMappingBytes.Length);
        Array.Copy(truePublicKeyBytes, 0, combinedForHash, lengthMappingBytes.Length, truePublicKeyBytes.Length);

        byte[] hashBytes = ComputeQuantumResistantHash(truePublicKeyBytes, combinedForHash);
        return CustomEncoder.Encode(hashBytes, addressLength);
    }
    
    private static int DetermineAddressLength(byte[] truePublicKeyBytes)
    {
        int sum = 0;
        int maxIndex = Math.Min(4, truePublicKeyBytes.Length);
        for (int i = 0; i < maxIndex; i++)
        {
            sum += truePublicKeyBytes[i];
        }
        return PublicKeyMinLength + (sum % (PublicKeyMaxLength - PublicKeyMinLength + 1));
    }
    
    private static byte[] ComputeQuantumResistantHash(byte[] key, byte[] data)
    {
        using var sha512 = SHA512.Create();
        byte[] layer1 = sha512.ComputeHash(data);
        
        byte[] saltedInput = new byte[layer1.Length + key.Length];
        Array.Copy(layer1, 0, saltedInput, 0, layer1.Length);
        Array.Copy(key, 0, saltedInput, layer1.Length, key.Length);
        
        byte[] layer2 = sha512.ComputeHash(saltedInput);
        
        byte[] combined = new byte[layer1.Length];
        for (int i = 0; i < combined.Length; i++)
        {
            combined[i] = (byte)(layer1[i] ^ layer2[i]);
        }
        
        return sha512.ComputeHash(combined);
    }
}
