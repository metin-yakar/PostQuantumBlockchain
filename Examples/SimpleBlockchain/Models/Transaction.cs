using BlockchainEngine;

namespace SimpleBlockchain.Models;

/// <summary>
/// Represents a cryptographically verifiable coin movement between two addresses.
/// </summary>
public class Transaction
{
    public string Sender { get; set; } = string.Empty;
    public string Receiver { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string SignaturePayload { get; set; } = string.Empty;

    /// <summary>
    /// Generates the deterministic format that must be hashed: Sender->Receiver:Amount
    /// </summary>
    public string GetSignableData()
    {
        return $"{Sender}->{Receiver}:{Amount}";
    }

    /// <summary>
    /// Packages the transaction variables through the core hashing mechanism using ECDSA.
    /// </summary>
    /// <param name="privateKey">The sender's private credentials</param>
    public void Sign(string privateKey)
    {
        string rawData = GetSignableData();
        this.SignaturePayload = SigningEngine.Sign(rawData, privateKey);
    }
}
