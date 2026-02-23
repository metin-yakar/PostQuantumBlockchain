using System;
using System.IO;
using System.Threading.Tasks;
using SimpleBlockchain.Models;
using SimpleBlockchain.Network;

namespace SimpleBlockchain;

class Program
{
    private static readonly string DataDirectory = "Data";
    private static readonly string ValidatorsFile = "validators.txt";

    static async Task Main(string[] args)
    {
        Console.WriteLine("=====================================================");
        Console.WriteLine("   SimpleBlockchain: Local P2P Node Demo             ");
        Console.WriteLine("=====================================================");
        
        int myPort = args.Length > 0 ? int.Parse(args[0]) : 5000;
        string myUrl = $"http://localhost:{myPort}/";

        // Initialize networking node
        var p2pNode = new P2PNode(DataDirectory, ValidatorsFile, myUrl);
        p2pNode.StartListening();

        var alice = new Wallet(100.0m);
        var bob = new Wallet(0.0m);

        Console.WriteLine($"[System] Alice Address: {alice.PublicAddress}");
        Console.WriteLine($"[System] Bob Address: {bob.PublicAddress}");

        while (true)
        {
            Console.WriteLine("\n[Options] 1: Broadcast Transaction  2: View Stored Data  3: Exit");
            string choice = Console.ReadLine();
            
            if (choice == "1")
            {
                try
                {
                    Transaction tx1 = alice.SendFunds(bob, 25.0m);
                    Console.WriteLine($"[System] Created Transaction. Base62 Sig: {tx1.SignaturePayload}");
                    
                    p2pNode.ProcessAndSaveTransaction(tx1, "Local");
                    await p2pNode.BroadcastTransactionAsync(tx1);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] {ex.Message}");
                }
            }
            else if (choice == "2")
            {
                string[] files = Directory.GetFiles(DataDirectory);
                Console.WriteLine($"[System] There are {files.Length} verified blockchain records in {DataDirectory}/");
                foreach (string f in files) 
                {
                    Console.WriteLine($"  - {f}");
                }
            }
            else if (choice == "3")
            {
                break;
            }
        }
        
        p2pNode.StopListening();
    }
}
