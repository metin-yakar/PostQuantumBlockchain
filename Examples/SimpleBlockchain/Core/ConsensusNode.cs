using BlockchainEngine;
using SimpleBlockchain.Models;

namespace SimpleBlockchain.Core;

/// <summary>
/// Represents a P2P Validator confirming the state logic against the engine.
/// </summary>
public class ConsensusNode
{
    /// <summary>
    /// Processes a transaction verifying cryptographic integrity without possessing the sender's Private Key.
    /// </summary>
    /// <param name="tx">The broadcasted Transaction object</param>
    /// <returns>True if the transaction was mathematically formed by the True Sender Address</returns>
    public bool ValidateTransaction(Transaction tx)
    {
        string rawData = tx.GetSignableData();

        // Ask BlockchainEngine to evaluate the cryptographic layer asymmetrically utilizing the public sender address
        string engineCode = SigningEngine.Verify(rawData, tx.SignaturePayload, tx.Sender);

        // Reject tampering
        if (engineCode == "0") return false;

        // Extract native logic: "1-[PUBLICKEY]-[TIMESTAMP]"
        var blocks = engineCode.Split('-');
        return blocks.Length >= 3 && blocks[0] == "1";
    }
}
