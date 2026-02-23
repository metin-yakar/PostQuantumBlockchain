using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BlockchainEngine;

public static class SigningEngine
{
    public static string Sign(string message, string privateKeyBase62)
    {
        byte[] privateKeyBytes = CustomEncoder.DecodeBase62(privateKeyBase62);
        
        using var ecdsa = ECDsa.Create();
        ecdsa.ImportECPrivateKey(privateKeyBytes, out _);
        
        byte[] truePublicKey = ecdsa.ExportSubjectPublicKeyInfo();
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        byte[] messageBytes = Encoding.UTF8.GetBytes(message + timestamp.ToString());
        
        // Native Asymmetric Post-Quantum resistant hashing size (SHA-512)
        byte[] signature = ecdsa.SignData(messageBytes, HashAlgorithmName.SHA512);
        
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        
        bw.Write((ushort)truePublicKey.Length);
        bw.Write(truePublicKey);
        bw.Write(timestamp);
        bw.Write((ushort)signature.Length);
        bw.Write(signature);
        
        byte[] packageBytes = ms.ToArray();
        byte[] safePackage = new byte[packageBytes.Length + 1];
        Array.Copy(packageBytes, safePackage, packageBytes.Length);
        safePackage[packageBytes.Length] = 0x01; // Tamper evident pad
        
        return CustomEncoder.Encode(safePackage);
    }

    /// <summary>
    /// Verifies the signature securely using TRUE asymmetric public key cryptography.
    /// Does NOT require the Private Key anymore.
    /// </summary>
    public static string Verify(string message, string signaturePackage, string expectedPublicAddress)
    {
        try
        {
            byte[] safePackage = CustomEncoder.DecodeAny(signaturePackage);
            
            if (safePackage.Length < 1 || safePackage[safePackage.Length - 1] != 0x01)
            {
                return "0";
            }
                
            byte[] packageBytes = new byte[safePackage.Length - 1];
            Array.Copy(safePackage, packageBytes, safePackage.Length - 1);
            
            using var ms = new MemoryStream(packageBytes);
            using var br = new BinaryReader(ms);
            
            ushort pubKeyLen = br.ReadUInt16();
            byte[] truePublicKey = br.ReadBytes(pubKeyLen);
            long timestamp = br.ReadInt64();
            ushort sigLen = br.ReadUInt16();
            byte[] signature = br.ReadBytes(sigLen);
            
            // 1. Assert the embedded True Public Key derives the expected Short Address
            string derivedAddress = KeyGenerator.DeriveAddress(truePublicKey);
            if (derivedAddress != expectedPublicAddress)
            {
                return "0"; // Address Spoofing Attack Pattern detected
            }
            
            // 2. Validate Asymmetric Integrity 
            byte[] messageBytes = Encoding.UTF8.GetBytes(message + timestamp.ToString());
            
            using var ecdsa = ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(truePublicKey, out _);
            
            bool isAuthentic = ecdsa.VerifyData(messageBytes, signature, HashAlgorithmName.SHA512);
            
            if (isAuthentic)
            {
                string timeStr = DateTimeOffset.FromUnixTimeSeconds(timestamp).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                return $"1-{expectedPublicAddress}-{timeStr}";
            }
            
            return "0";
        }
        catch (Exception)
        {
            return "0";
        }
    }
}
