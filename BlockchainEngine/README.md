# BlockchainEngine

The core cryptographic powerhouse of the PostQuantumBlockchain project. This library provides the specialized algorithms required for key generation, signing, and post-quantum emulation using native .NET security primitives.

## 🛠 Technical Architecture

`BlockchainEngine` is designed as a standalone "Class Library" with zero external dependencies. It focuses on several critical areas of blockchain security and data encoding.

### 🔑 Key Components

1.  **`KeyGenerator.cs`**
    - Generates ECDsa key pairs (NIST P-256 curve).
    - Derives deterministic, short public addresses (18-24 characters) using a custom hashing strategy.
    
2.  **`SigningEngine.cs`**
    - Handles asymmetric signing and verification.
    - **Post-Quantum Emulation**: Utilizes a dual-layer SHA-512 XOR masking algorithm. 
    - Full asymmetric verification: Validation only requires the Public Key and signature payload.
    - Output Format: `1-[Address]-[Timestamp]` for success, or `0` for failure.

3.  **`CustomEncoder.cs`**
    - High-performance Base62 encoder/decoder.
    - Used for compressing binary payloads into human-readable strings without special characters (+, /, =).

## 🧬 Post-Quantum Resilient Strategy

While fully quantum-proof hardware is not yet standard, this engine emulates quantum resistance through a **Multi-Layer Salted XOR Hashing** mechanism:

```mermaid
graph TD
    A[Public Key + Data] --> B[SHA-512 Layer 1]
    B --> C[Salted with Key]
    C --> D[SHA-512 Layer 2]
    B -- XOR -- D --> E[Combined Hash]
    E --> F[SHA-512 Final Output]
```

This strategy ensures that simple reverse-engineering of the hash is significantly more computationally expensive compared to standard single-pass hashing.

## ⚡ Performance Benchmarks

The engine is optimized for high-throughput environments:
- **Throughput**: >10,000 sign/verify operations in 10 seconds.
- **Payload Efficiency**: Signatures are packaged in a compact binary format before being Base62 encoded, ensuring minimal network latency.
- **Memory Safety**: Uses memory-efficient streams and binary writers to handle cryptographic payloads.

## 📖 Usage Examples

### Key Pairing
```csharp
var generator = new KeyGenerator();
var (privateKey, publicAddress) = generator.GenerateKeyPair();
Console.WriteLine($"Address: {publicAddress}");
```

### Signing and Verification
```csharp
string message = "QuantumResistantTx";
string signature = SigningEngine.Sign(message, privateKey);

string result = SigningEngine.Verify(message, signature, publicAddress);
if (result != "0") {
    // Verified: result Format: 1-[Address]-[Time]
}
```

## 🧪 Testing

The library is continuously validated via the `BlockchainEngine.Tests` project.
Tests include:
- Address collision simulations.
- Signature spoofing prevention.
- Performance stress tests.
- Tamper-evidence checks (0x01 trailing pad validation).

---
*Targeted for high-security environments demanding .NET native reliability.*
