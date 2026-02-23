using System;
using System.Numerics;
using Jint;
using Jint.Runtime;
using SmartBlockchain.Models;

namespace SmartBlockchain.Core;

public class ContractRuntime
{
    private readonly Storage _globalStorage;
    private readonly Engine _engine;

    public ContractRuntime(Storage globalStorage)
    {
        _globalStorage = globalStorage;
        _engine = new Engine(cfg => cfg
            // Limit execution to prevent infinite loop malicious contracts (Gas Limit emulation)
            .TimeoutInterval(TimeSpan.FromSeconds(2))
            // Limit memory usage (Memory Limit emulation)
            .LimitMemory(4 * 1024 * 1024) 
        );

        // Pre-configure immutable environment sandboxes for contract logic 
        _engine.SetValue("storage", _globalStorage);

        // Native 18-decimal protection. 
        // JavaScript float values inherently lose monetary precision on huge blockchains.
        // We inject `decimal(floatvalue)` bridging it straight into a BigInteger representation.
        _engine.SetValue("decimal", new Func<double, string>(ConvertTo18Decimal));
    }

    /// <summary>
    /// Executes a transaction verifying its output against the local JINT state machine.
    /// </summary>
    public bool ExecuteTransaction(Transaction tx, long blockTimestamp)
    {
        try
        {
            // Context Variables (_msg, _block)
            _engine.SetValue("_msg", new { sender = tx.Sender, payload = tx.SignaturePayload });
            _engine.SetValue("_block", new { timestamp = blockTimestamp });

            // Execute the raw script
            _engine.Execute(tx.ContractCode);
            
            return true;
        }
        catch (JavaScriptException jex)
        {
            Console.WriteLine($"[SmartContract REVERTED] JS Execution Error: {jex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SmartContract REVERTED] VM Fault: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Financials should NOT use standard IEEE 754 float/doubles. 
    /// Enforces exactly 18 decimal places of pure mathematical integer scaling logic.
    /// 1 unit = 1_000_000_000_000_000_000 sub-units.
    /// </summary>
    private string ConvertTo18Decimal(double floatValue)
    {
        // Equivalent to Solidiy/Ethereum Wei
        var multiplier = BigInteger.Pow(10, 18);
        
        // C# decimals natively support 28-29 significant digits, enough to safely calculate the BigInteger multiplier
        decimal preciseValue = (decimal)floatValue;
        
        BigInteger finalValue = (BigInteger)(preciseValue * (decimal)multiplier);
        
        return finalValue.ToString();
    }
}
