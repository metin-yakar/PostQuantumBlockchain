using System;
using BlockchainEngine;
using SmartBlockchain.Models;

namespace SmartBlockchain.Core;

/// <summary>
/// Combines mathematical Cryptographic checks with deterministic Javascript execution.
/// </summary>
public class ConsensusNode
{
    private readonly ContractRuntime _runtime;

    public ConsensusNode(Storage globalStorage)
    {
        _runtime = new ContractRuntime(globalStorage);
    }

    /// <summary>
    /// Evaluates if the packet is genuine via ECDsa verification, then executes the Smart Contract sandbox.
    /// </summary>
    public bool ValidateAndExecute(Transaction tx)
    {
        string rawData = tx.GetSignableData();

        // 1. Cryptographic Authentication: Is this literally signed by the Sender Address?
        string engineCode = SigningEngine.Verify(rawData, tx.SignaturePayload, tx.Sender);

        if (engineCode == "0") return false;

        // Verify Output schema: 1-[PUBLICKEY]-[yyyy-MM-dd HH:mm:ss]
        var blocks = engineCode.Split('-');
        if (blocks.Length < 3 || blocks[0] != "1") return false;

        // 2. Deterministic VM Execution: Cryptography passed! Let's process the javascript payload.
        long timeReference = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        return _runtime.ExecuteTransaction(tx, timeReference);
    }
}
