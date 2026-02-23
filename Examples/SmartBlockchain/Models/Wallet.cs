using System;
using BlockchainEngine;

namespace SmartBlockchain.Models;

/// <summary>
/// Represents a network participant capable of deploying Smart Contracts.
/// </summary>
public class Wallet
{
    public string PublicAddress { get; private set; }
    public string PrivateKey { get; private set; }

    public Wallet()
    {
        var generator = new KeyGenerator();
        var keys = generator.GenerateKeyPair();

        this.PrivateKey = keys.PrivateKey;
        this.PublicAddress = keys.PublicKey;
    }

    /// <summary>
    /// Creates and signs a Smart Contract transaction payload.
    /// </summary>
    /// <param name="jsCode">The JavaScript contract code</param>
    /// <returns>A cryptographically signed Transaction payload ready for P2P broadcast</returns>
    public Transaction DeployContract(string jsCode)
    {
        var tx = new Transaction
        {
            Sender = this.PublicAddress,
            ContractCode = jsCode
        };

        // Signs using the exact Private Key exported from ECDSA mathematics
        tx.Sign(this.PrivateKey);
        return tx;
    }
}
