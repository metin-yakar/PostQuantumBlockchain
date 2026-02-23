using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SmartBlockchain.Core;
using SmartBlockchain.Models;

namespace SmartBlockchain.Network;

/// <summary>
/// Handles asynchronous TCP/HTTP broadcasting of Smart Contract JSON packets.
/// </summary>
public class P2PNode
{
    private readonly string _dataDirectory;
    private readonly string _validatorsFile;
    private readonly HttpClient _http;
    private readonly HttpListener _listener;
    private readonly ConsensusNode _validatorNode;

    public P2PNode(string dataDirectory, string validatorsFile, string myUrl, Storage globalStorage)
    {
        _dataDirectory = dataDirectory;
        _validatorsFile = validatorsFile;
        _http = new HttpClient();
        _validatorNode = new ConsensusNode(globalStorage);
        
        Directory.CreateDirectory(_dataDirectory);
        
        if (!File.Exists(_validatorsFile))
        {
            File.WriteAllText(_validatorsFile, "http://localhost:6001/\nhttp://localhost:6002/\n");
        }

        _listener = new HttpListener();
        _listener.Prefixes.Add(myUrl);
    }

    /// <summary>
    /// Triggers asynchronous execution checking the network for REST hooks.
    /// </summary>
    public void StartListening()
    {
        _listener.Start();
        Console.WriteLine($"[Node] Network Listener Running on {_listener.Prefixes.Count} prefix(es)");
        
        _ = Task.Run(async () =>
        {
            while (_listener.IsListening)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    var req = context.Request;
                    
                    if (req.HttpMethod == "POST" && req.RawUrl == "/tx")
                    {
                        using var reader = new StreamReader(req.InputStream);
                        string body = await reader.ReadToEndAsync();
                        var incomingTx = JsonSerializer.Deserialize<Transaction>(body);
                        
                        if (incomingTx != null)
                        {
                            ProcessAndSaveTransaction(incomingTx, "P2P Network");
                        }
                        context.Response.StatusCode = 200;
                    }
                    else
                    {
                        context.Response.StatusCode = 404;
                    }
                    context.Response.Close();
                }
                catch { /* Quiet fallback gracefully */ }
            }
        });
    }

    public void StopListening()
    {
        _listener.Stop();
    }

    /// <summary>
    /// Processes cryptographic hashes and routes correct code into Javascript Sandbox, saving to filesystem if valid.
    /// </summary>
    public void ProcessAndSaveTransaction(Transaction tx, string source)
    {
        bool isAuthentic = _validatorNode.ValidateAndExecute(tx);

        if (isAuthentic)
        {
            string txPath = Path.Combine(_dataDirectory, $"tx_{tx.SignaturePayload}.json");
            if (!File.Exists(txPath))
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(txPath, JsonSerializer.Serialize(tx, options));
                Console.WriteLine($"[SmartConsensus] Evaluated JS Contract (Asymmetric). Received from {source}. Saved block at {txPath}");
            }
            else
            {
                Console.WriteLine($"[SmartConsensus] Ignoring Duplicate Contract Code Transfer.");
            }
        }
        else
        {
            Console.WriteLine($"[SmartConsensus] WARNING: REJECTED TAMPERED CONTRACT PAYLOAD FROM {source}.");
        }
    }

    /// <summary>
    /// Broadcasts local interactions across registered node validators.
    /// </summary>
    public async Task BroadcastTransactionAsync(Transaction tx)
    {
        if (!File.Exists(_validatorsFile)) return;
        
        string[] peers = File.ReadAllLines(_validatorsFile);
        string payload = JsonSerializer.Serialize(tx);
        var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

        foreach (var peer in peers)
        {
            if (string.IsNullOrWhiteSpace(peer)) continue;
            try
            {
                string targetUrl = peer.TrimEnd('/') + "/tx";
                await _http.PostAsync(targetUrl, content);
                Console.WriteLine($"[Broadcast] Transferred smart contract to Peer => {targetUrl}");
            }
            catch
            {
                Console.WriteLine($"[Broadcast] Failed to connect to offline Peer => {peer}");
            }
        }
    }
}
