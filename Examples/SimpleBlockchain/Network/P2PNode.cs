using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SimpleBlockchain.Core;
using SimpleBlockchain.Models;

namespace SimpleBlockchain.Network;

/// <summary>
/// Handles asynchronous TCP/HTTP broadcasting and reception of serialized blockchain transactions.
/// </summary>
public class P2PNode
{
    private readonly string _dataDirectory;
    private readonly string _validatorsFile;
    private readonly HttpClient _http;
    private readonly HttpListener _listener;
    private readonly ConsensusNode _validatorNode;

    public P2PNode(string dataDirectory, string validatorsFile, string myUrl)
    {
        _dataDirectory = dataDirectory;
        _validatorsFile = validatorsFile;
        _http = new HttpClient();
        _validatorNode = new ConsensusNode();
        
        Directory.CreateDirectory(_dataDirectory);
        
        if (!File.Exists(_validatorsFile))
        {
            File.WriteAllText(_validatorsFile, "http://localhost:5001/\nhttp://localhost:5002/\n");
        }

        _listener = new HttpListener();
        _listener.Prefixes.Add(myUrl);
    }

    /// <summary>
    /// Starts the background listener thread to receive P2P broadcasts.
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
                catch { /* Ignore listener exceptions */ }
            }
        });
    }

    public void StopListening()
    {
        _listener.Stop();
    }

    /// <summary>
    /// Evaluates cryptographic logic and permanently writes the transaction if correct.
    /// </summary>
    public void ProcessAndSaveTransaction(Transaction tx, string source)
    {
        bool isAuthentic = _validatorNode.ValidateTransaction(tx);

        if (isAuthentic)
        {
            string txPath = Path.Combine(_dataDirectory, $"tx_{tx.SignaturePayload}.json");
            if (!File.Exists(txPath))
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(txPath, JsonSerializer.Serialize(tx, options));
                Console.WriteLine($"[Consensus] Transaction Verified (Asymmetric). Received from {source}. Saved block at {txPath}");
            }
            else
            {
                Console.WriteLine($"[Consensus] Ignoring Duplicate Block Transfer: tx_{tx.SignaturePayload}.json");
            }
        }
        else
        {
            Console.WriteLine($"[Consensus] WARNING: REJECTED TAMPERED PAYLOAD FROM {source}.");
        }
    }

    /// <summary>
    /// Synchronizes the payload to all peers registered in the internal validator ledger.
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
                Console.WriteLine($"[Broadcast] Transferred packet to Peer => {targetUrl}");
            }
            catch
            {
                Console.WriteLine($"[Broadcast] Failed to connect to offline Peer => {peer}");
            }
        }
    }
}
