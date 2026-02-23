using System;
using System.Numerics;

namespace BlockchainEngine;

public static class CustomEncoder
{
    private const string CharacterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    
    public static string Encode(byte[] data, int outputLength)
    {
        if (data == null || data.Length == 0) throw new ArgumentException("Data cannot be null or empty", nameof(data));
        if (outputLength <= 0) throw new ArgumentException("Output length must be positive", nameof(outputLength));

        byte[] positiveBytes = new byte[data.Length + 1];
        Array.Copy(data, positiveBytes, data.Length);
        positiveBytes[data.Length] = 0x00; 
        
        BigInteger number = new BigInteger(positiveBytes);
        var result = new char[outputLength];
        int baseValue = CharacterSet.Length; 
        
        for (int i = outputLength - 1; i >= 0; i--)
        {
            number = BigInteger.DivRem(number, baseValue, out BigInteger remainder);
            result[i] = CharacterSet[(int)remainder];
        }
        
        return new string(result);
    }
    
    public static string Encode(byte[] data)
    {
        int outputLength = (int)Math.Ceiling(data.Length * 8.0 / Math.Log2(62));
        return Encode(data, outputLength);
    }

    public static byte[] DecodeBase62(string encoded)
    {
        return DecodeAny(encoded);
    }

    public static byte[] DecodeAny(string encoded)
    {
        BigInteger number = 0;
        BigInteger baseValue = 62;

        for (int i = 0; i < encoded.Length; i++)
        {
            char c = encoded[i];
            int val = CharacterSet.IndexOf(c);
            if (val < 0) throw new FormatException($"Invalid Base62 character: {c}");
            number = number * baseValue + val;
        }

        byte[] decodedBytes = number.ToByteArray();
        
        if (decodedBytes.Length > 0 && decodedBytes[decodedBytes.Length - 1] == 0x00)
        {
            byte[] result = new byte[decodedBytes.Length - 1];
            Array.Copy(decodedBytes, result, result.Length);
            return result;
        }
        
        return decodedBytes;
    }
}
