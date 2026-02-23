# PostQuantumBlockchain

A professional-grade, .NET 8-based blockchain infrastructure designed with Post-Quantum resistance and Clean Architecture principles. This project serves as a foundational library for building secure, scalable, and modular blockchain systems.

## 🚀 Overview

**PostQuantumBlockchain** is a modern blockchain experiment focusing on asymmetric cryptography that emulates quantum-resistant patterns. Unlike traditional blockchain libraries, this project is built from the ground up without external dependencies like NBitcoin or BouncyCastle, utilizing native C# security primitives combined with a custom SHA-512 XOR-protected masking layer.

### Key Pillars
- **Post-Quantum Resilience**: Custom hashing strategy using multi-layer SHA-512 with XOR masking to protect against traditional derivation attacks.
- **Asymmetric Integrity**: Full support for `.NET ECDsa` signing with isolated private/public key mechanics.
- **Clean Architecture**: Decoupled modules for Core, Network, and Models ensuring maintainability and testability.
- **Micro-Public Addresses**: Unique Base62-encoded public addresses (18-24 characters) derived through deterministic collision-resistant mapping.

---

## 🏗 Project Structure

The repository is organized into a modular hierarchy:

| Directory | Description |
| :--- | :--- |
| **`BlockchainEngine/`** | The core library containing cryptography, encoding, and signing logic. |
| **`Examples/SimpleBlockchain/`** | A modular P2P console application demonstrating transaction persistence and broadcasting. |
| **`Examples/SmartBlockchain/`** | A Smart Contract VM implementation using Jint Sandbox with high-precision decimal support. |
| **`BlockchainEngine.Tests/`** | Extensive xUnit test suite validating performance and security benchmarks. |

---

## 💎 Features

- **🌐 P2P Networking**: Full node implementation with peer discovery via `validators.txt` and HTTP-based transaction broadcasting.
- **📜 Smart Contracts**: Execute sandboxed JavaScript logic with isolated state storage and prevention of floating-point math errors.
- **⚡ Performance-First**: Capable of processing over 10,000 signature verifications in under 10 seconds on standard hardware.
- **🔒 Zero-Dependency**: Strictly utilizes native `System.Security.Cryptography` to minimize supply-chain risk.

---

## 🛠 Tech Stack

- **Framework**: .NET 8.0 SDK
- **Language**: C# 12.0
- **Cryptography**: ECDsa (P-256), SHA-512
- **Testing**: xUnit, FluentAssertions (optional)
- **Documentation**: Mermaid diagrams for architectural visualization.

---

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Installation
```bash
git clone https://github.com/metin-yakar/PostQuantumBlockchain.git
cd PostQuantumBlockchain
dotnet build
```

### Running the Examples
1. **Simple P2P Node**:
   ```bash
   cd Examples/SimpleBlockchain
   dotnet run
   ```

2. **Smart Contract VM**:
   ```bash
   cd Examples/SmartBlockchain
   dotnet run
   ```

---

## 📄 License
This project is open-source. See the repository for license details.

---
*Maintained for Academic and Research Purposes.*
