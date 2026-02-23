using System;
using System.Diagnostics;
using System.Linq;
using Xunit;
using BlockchainEngine;

namespace BlockchainEngine.Tests;

public class EngineTests
{
    [Fact]
    public void GenerateKeyPair_ShouldProduceUniqueSets()
    {
        var generator = new KeyGenerator();
        int iterations = 1000;
        var privateKeys = new string[iterations];
        var publicKeys = new string[iterations];

        for (int i = 0; i < iterations; i++)
        {
            var pair = generator.GenerateKeyPair();
            privateKeys[i] = pair.PrivateKey;
            publicKeys[i] = pair.PublicKey;
        }

        var distinctPrivates = privateKeys.Distinct().Count();
        var distinctPublics = publicKeys.Distinct().Count();

        Assert.Equal(iterations, distinctPrivates);
        Assert.Equal(iterations, distinctPublics);
    }

    [Fact]
    public void PublicKey_LengthShouldBeBetween18And24()
    {
        var generator = new KeyGenerator();
        for (int i = 0; i < 500; i++)
        {
            var pair = generator.GenerateKeyPair();
            Assert.InRange(pair.PublicKey.Length, 18, 24);
        }
    }

    [Fact]
    public void SignAndVerify_ValidSignature_ShouldReturnSuccess()
    {
        var generator = new KeyGenerator();
        var pair = generator.GenerateKeyPair();
        string message = "AcademicTestMessage2026";
        
        string signature = SigningEngine.Sign(message, pair.PrivateKey);
        
        Assert.False(string.IsNullOrEmpty(signature));
        Assert.False(signature.Contains("|"));
        Assert.False(signature.Contains("-"));
        
        string result = SigningEngine.Verify(message, signature, pair.PublicKey);
        Assert.StartsWith("1-", result);
        Assert.Contains($"-{pair.PublicKey}-", result);
    }

    [Fact]
    public void Verify_InvalidMessage_ShouldReturnZero()
    {
        var generator = new KeyGenerator();
        var pair = generator.GenerateKeyPair();
        string message = "ValidMessage";
        string tamperedMessage = "InvalidMessage";
        
        string signature = SigningEngine.Sign(message, pair.PrivateKey);
        
        string result = SigningEngine.Verify(tamperedMessage, signature, pair.PublicKey);
        Assert.Equal("0", result);
    }

    [Fact]
    public void Verify_InvalidPublicKey_ShouldReturnZero()
    {
        var generator = new KeyGenerator();
        var pair1 = generator.GenerateKeyPair();
        var pair2 = generator.GenerateKeyPair();
        string message = "TestMessage";
        
        string signature = SigningEngine.Sign(message, pair1.PrivateKey);
        
        string result = SigningEngine.Verify(message, signature, pair2.PublicKey);
        Assert.Equal("0", result);
    }

    [Fact]
    public void Verify_TamperedSignature_ShouldReturnZero()
    {
        var generator = new KeyGenerator();
        var pair = generator.GenerateKeyPair();
        string message = "TestMessage";
        
        string signature = SigningEngine.Sign(message, pair.PrivateKey);
        
        // Tamper signature by modifying the last character
        char[] sigChars = signature.ToCharArray();
        sigChars[sigChars.Length - 1] = sigChars[sigChars.Length - 1] == 'A' ? 'B' : 'A';
        string tamperedSignature = new string(sigChars);
        
        string result = SigningEngine.Verify(message, tamperedSignature, pair.PublicKey);
        Assert.Equal("0", result);
    }

    [Fact]
    public void MassSigningTurnaround_ShouldExecuteConsistently()
    {
        var generator = new KeyGenerator();
        var stopwatch = new Stopwatch();
        int targetOperations = 1000;
        
        var pair = generator.GenerateKeyPair();
        
        stopwatch.Start();
        for (int i = 0; i < targetOperations; i++)
        {
            string signature = SigningEngine.Sign($"LoadTest{i}", pair.PrivateKey);
            Assert.NotNull(signature);
        }
        stopwatch.Stop();
        
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, "Signing 1000 times took too long.");
    }
}
