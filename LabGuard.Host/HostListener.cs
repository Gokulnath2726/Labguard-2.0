using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using LabGuard.Common;

namespace LabGuard.Host
{
    public class ClientSession
    {
        public string Id { get; set; }
        public string Hostname { get; set; }
        public TcpClient TcpClient { get; set; }
        public NetworkStream Stream { get; set; }
        public DateTime LastHeartbeat { get; set; }
        public ClientStatus Status { get; set; }
        public string? Details { get; set; }

        public ClientSession(string id, TcpClient tcp, NetworkStream stream)
        {
            Id = id;
            Hostname = id;
            TcpClient = tcp;
            Stream = stream;
            LastHeartbeat = DateTime.UtcNow;
            Status = ClientStatus.Normal;
        }
    }

    public class HostListener
    {
        private readonly TcpListener _listener;
        private readonly Dictionary<string, ClientSession> _clients = new();
        public event Action<ClientInfo>? OnClientConnected;
        public event Action<string>? OnClientDisconnected;
        public event Action<string, string>? OnClientStatusUpdated;

        public HostListener(int port = 9000)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            _listener.Start();
            Console.WriteLine("Host listening on port 9000...");
            Task.Run(AcceptLoop);
            Task.Run(MonitorClientHealth);
        }

        public IEnumerable<ClientSession> GetConnectedClients()
        {
            lock (_clients)
            {
                return _clients.Values.ToList();
            }
        }

        public Task SendCommandToClient(string clientId, CommandMessage command)
        {
            lock (_clients)
            {
                if (!_clients.TryGetValue(clientId, out var session))
                {
                    Console.WriteLine($"Client {clientId} not found");
                    return Task.CompletedTask;
                }

                try
                {
                    var data = MessageSerializer.Serialize(command);
                    _ = session.Stream.WriteAsync(data, 0, data.Length);
                    Console.WriteLine($"Command {command.Command} sent to {clientId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending command to {clientId}: {ex.Message}");
                }
            }
            return Task.CompletedTask;
        }

        private async Task AcceptLoop()
        {
            while (true)
            {
                try
                {
                    var tcp = await _listener.AcceptTcpClientAsync();
                    _ = HandleClient(tcp);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Accept error: {ex.Message}");
                }
            }
        }

        private async Task HandleClient(TcpClient tcp)
        {
            NetworkStream? stream = null;
            string? clientId = null;

            try
            {
                stream = tcp.GetStream();
                var lenBuf = new byte[4];

                while (true)
                {
                    // Read message length
                    int read = 0;
                    while (read < 4)
                    {
                        var r = await stream.ReadAsync(lenBuf, read, 4 - read);
                        if (r == 0) break;
                        read += r;
                    }
                    if (read == 0) break;

                    if (!BitConverter.IsLittleEndian) Array.Reverse(lenBuf);
                    var len = BitConverter.ToInt32(lenBuf, 0);
                    if (len > 1024 * 1024) break; // 1MB max

                    // Read message payload
                    var payload = new byte[len];
                    read = 0;
                    while (read < len)
                    {
                        var r = await stream.ReadAsync(payload, read, len - read);
                        if (r == 0) break;
                        read += r;
                    }
                    if (read == 0) break;

                    // Deserialize message
                    var msg = MessageSerializer.DeserializeFromSpan(payload);
                    if (msg == null) continue;

                    // Validate PSK
                    if (msg.Psk != Protocol.PSK)
                    {
                        Console.WriteLine($"Invalid PSK from {msg.SenderId}");
                        break;
                    }

                    if (msg is StatusUpdateMessage status)
                    {
                        clientId = status.SenderId;

                        lock (_clients)
                        {
                            if (!_clients.ContainsKey(clientId))
                            {
                                var session = new ClientSession(clientId, tcp, stream);
                                _clients[clientId] = session;
                                OnClientConnected?.Invoke(new ClientInfo(clientId, clientId, ClientStatus.Normal));
                                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Client {clientId} connected");
                            }

                            _clients[clientId].LastHeartbeat = DateTime.UtcNow;
                            _clients[clientId].Status = status.Status;
                            _clients[clientId].Details = status.Details;
                        }

                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Status from {clientId}: {status.Status} - {status.Details}");
                        OnClientStatusUpdated?.Invoke(clientId, status.Details ?? "");
                    }
                    else if (msg is BaseMessage ack && ack.MessageType == MessageType.Acknowledgement)
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Acknowledgement from {ack.SenderId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
            finally
            {
                if (clientId != null)
                {
                    lock (_clients)
                    {
                        _clients.Remove(clientId);
                    }
                    OnClientDisconnected?.Invoke(clientId);
                    Console.WriteLine($"Client {clientId} disconnected");
                }
                stream?.Dispose();
                tcp?.Dispose();
            }
        }

        private async Task MonitorClientHealth()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Monitor error: {ex.Message}");
                    continue;
                }

                lock (_clients)
                {
                    var staleClients = _clients
                        .Where(kvp => DateTime.UtcNow - kvp.Value.LastHeartbeat > TimeSpan.FromSeconds(60))
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var clientId in staleClients)
                    {
                        _clients.Remove(clientId);
                        OnClientDisconnected?.Invoke(clientId);
                        Console.WriteLine($"Client {clientId} marked offline (timeout)");
                    }
                }
            }
        }
    }
}
