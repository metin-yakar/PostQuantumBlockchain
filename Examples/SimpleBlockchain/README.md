# SimpleBlockchain: Example Console Ledger

This directory contains a standalone `.NET 8` console application designed to strictly demonstrate the real-world implementation of the `BlockchainEngine` core library. 

## 📌 Architecture Overview

In a decentralized environment, relying upon the `BlockchainEngine` for cryptographic fidelity requires abstracting logic into specific Domain Models. This example provides three primary abstractions natively interacting with our Post-Quantum engine:

1. **`Wallet`**: Demonstrates the generation of `PublicKey` addresses mapped against isolated `PrivateKey` entities. Calculates balances and invokes outgoing transactions.
2. **`Transaction`**: Replicates a standard UTXO / Account-based payload. Concatenates details deterministically (e.g. `Sender->Receiver:Amount`) and invokes `SigningEngine.Sign()` generating the Base62 string.
3. **`ConsensusNode`**: Represents a Miner or Validator evaluating broadcasts. Invokes `SigningEngine.Verify()` expecting the stringent `"1-[PUBKEY]-[TIMESTAMP]"` string natively parsed.

## 🚀 How to Run

1. Open your terminal in this `Examples/SimpleBlockchain/` directory.
2. Execute the application:
   ```bash
   dotnet run
   ```
3. The shell will simulate a local network generating wallets for Alice, Bob, and Charlie, construct a signed transaction, push it to the node, execute Post-Quantum cryptography, and finally alter the simulated Balances on success.

## ⚠️ Notes for Developers & Custom LLMs
This project is linked directly to the `../../BlockchainEngine` source file compilation path via `.csproj` references to prevent package contamination. This isolates the implementation entirely allowing for native debugging deep into the `CustomEncoder` mechanics.
