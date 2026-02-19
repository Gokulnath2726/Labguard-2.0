using System;
using System.Threading.Tasks;
using System.IO;

namespace LabGuard.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== LabGuard Client Agent ===");
            Console.WriteLine($"Host: {Environment.MachineName}");
            Console.WriteLine();

            // Ask for Host IP address
            string hostIP = "127.0.0.1";  // Default
            string lastIpFile = "last_host.txt";

            if (File.Exists(lastIpFile))
            {
                string savedIp = File.ReadAllText(lastIpFile).Trim();
                if (!string.IsNullOrWhiteSpace(savedIp))
                {
                    hostIP = savedIp;
                }
            }
            
            if (args.Length > 0)
            {
                // If IP passed as argument
                hostIP = args[0];
                Console.WriteLine($"Connecting to Host: {hostIP}");
            }
            else
            {
                // Ask user to enter Host IP
                Console.Write($"Enter Host IP Address (default {hostIP}): ");
                string? input = Console.ReadLine();
                
                if (!string.IsNullOrWhiteSpace(input))
                {
                    hostIP = input.Trim();
                }
                
                // Save the new IP
                File.WriteAllText(lastIpFile, hostIP);
            }

            Console.WriteLine($"Connecting to: {hostIP}:9000");
            Console.WriteLine();

            var client = new NetworkClient(hostIP, 9000);
            try
            {
                await client.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start: {ex.Message}");
                return;
            }

            Console.WriteLine("Press Ctrl+C to exit");
            await Task.Delay(-1);
        }
    }
}

