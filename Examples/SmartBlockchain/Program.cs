using System;
using System.IO;
using System.Threading.Tasks;
using SmartBlockchain.Models;
using SmartBlockchain.Network;

namespace SmartBlockchain;

class Program
{
    private static readonly string DataDirectory = "Data";
    private static readonly string ValidatorsFile = "validators.txt";

    static async Task Main(string[] args)
    {
        Console.WriteLine("=====================================================");
        Console.WriteLine("  SmartBlockchain: JINT Smart Contract Executor Demo ");
        Console.WriteLine("=====================================================");
        
        int myPort = args.Length > 0 ? int.Parse(args[0]) : 6000;
        string myUrl = $"http://localhost:{myPort}/";

        // Memory database representing global smart contract states
        var globalStorage = new Storage();

        // Initialize networking node
        var p2pNode = new P2PNode(DataDirectory, ValidatorsFile, myUrl, globalStorage);
        p2pNode.StartListening();

        var developerWallet = new Wallet();

        Console.WriteLine($"[System] Dev Address: {developerWallet.PublicAddress}");

        while (true)
        {
            Console.WriteLine("\n[Options] 1: Deploy Token Contract  2: View Storage State  3: View Saved Code Blocks  4: Exit");
            string choice = Console.ReadLine();
            
            if (choice == "1")
            {
                try
                {
                    // Advanced JINT Javascript Smart Contract
                    // Enforces global Memory Storage mutations and 18-decimal strict mathematics.
                    string javascriptCode = @"
                        // Native precision cast (18-decimal trailing)
                        var reward = decimal(5.50); 
                        
                        // Storage tracking per Sender Address
                        var currentBal = storage.has(_msg.sender) ? storage.get(_msg.sender) : '0';
                        
                        // Emit
                        storage.set(_msg.sender, reward);
                        storage.set('LastTxTimestamp', _block.timestamp.toString());
                    ";

                    Console.WriteLine("[System] Assembling Javascript Contract...");
                    Transaction txContract = developerWallet.DeployContract(javascriptCode.Trim());
                    Console.WriteLine($"[System] Cryptographic Payload Hashed. Base62 Sig: {txContract.SignaturePayload}");
                    
                    p2pNode.ProcessAndSaveTransaction(txContract, "Local");
                    await p2pNode.BroadcastTransactionAsync(txContract);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] {ex.Message}");
                }
            }
            else if (choice == "2")
            {
                Console.WriteLine($"[State] Internal Javascript Storage State:");
                Console.WriteLine($"  - Sender Balance: {globalStorage.get(developerWallet.PublicAddress)}");
                Console.WriteLine($"  - Last TX Timestamp: {globalStorage.get("LastTxTimestamp")}");
            }
            else if (choice == "3")
            {
                string[] files = Directory.GetFiles(DataDirectory);
                Console.WriteLine($"[System] There are {files.Length} cryptographically verified JS contracts stored in {DataDirectory}/");
            }
            else if (choice == "4")
            {
                break;
            }
        }
        
        p2pNode.StopListening();
    }
}
