using System;
using BlockchainEngine;

namespace SimpleBlockchain.Models;

/// <summary>
/// Represents a network participant capable of holding balances and signing transactions.
/// </summary>
public class Wallet
{
    public string PublicAddress { get; private set; }
    public string PrivateKey { get; private set; }
    public decimal Balances { get; set; }

    public Wallet(decimal initialBalance = 0)
    {
        var generator = new KeyGenerator();
        var keys = generator.GenerateKeyPair();

        this.PrivateKey = keys.PrivateKey;
        this.PublicAddress = keys.PublicKey;
        this.Balances = initialBalance;
    }

    /// <summary>
    /// Creates and signs a transaction to another participant.
    /// </summary>
    /// <param name="recipient">The wallet receiving the coins</param>
    /// <param name="amount">The decimal amount to transfer</param>
    /// <returns>A cryptographically signed Transaction payload ready for P2P broadcast</returns>
    public Transaction SendFunds(Wallet recipient, decimal amount)
    {
        if (this.Balances < amount)
        {
            throw new InvalidOperationException($"Insufficient Funds in {this.PublicAddress}");
        }

        var tx = new Transaction
        {
            Sender = this.PublicAddress,
            Receiver = recipient.PublicAddress,
            Amount = amount
        };

        // Signs using the exact Private Key exported from ECDSA mathematics
        tx.Sign(this.PrivateKey);
        return tx;
    }
}
