using System.Collections.Generic;

namespace SmartBlockchain.Models;

/// <summary>
/// A sandboxed environment exposing Key-Value functionality safely to the JavaScript execution engine.
/// Provides native access to 'storage' internally mapped to a secure C# dictionary.
/// </summary>
public class Storage
{
    // Real-world blockchains would persist this via LevelDB or RocksDB. We use memory for the example.
    private readonly Dictionary<string, string> _state = new();

    public void set(string key, string value)
    {
        _state[key] = value;
    }

    public string get(string key)
    {
        return _state.ContainsKey(key) ? _state[key] : string.Empty;
    }

    public bool has(string key)
    {
        return _state.ContainsKey(key);
    }
}
