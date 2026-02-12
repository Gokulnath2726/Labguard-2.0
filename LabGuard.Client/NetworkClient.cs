using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LabGuard.Common;

namespace LabGuard.Client
{
    public class NetworkClient
    {
        private readonly string _host;
        private readonly int _port;
        private TcpClient? _tcp;
        private NetworkStream? _stream;
        private CancellationTokenSource? _cts;

        public NetworkClient(string host = "127.0.0.1", int port = 9000)
        {
            _host = host;
            _port = port;
        }

        public async Task StartAsync()
        {
            _tcp = new TcpClient();
            _cts = new CancellationTokenSource();

            try
            {
                await _tcp.ConnectAsync(_host, _port);
                _stream = _tcp.GetStream();
                Console.WriteLine("Connected to host");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
                throw;
            }

            // Start both heartbeat and command receive loops concurrently
            var heartbeatTask = SendHeartbeatLoop(_cts.Token);
            var receiveTask = ReceiveCommandLoop(_cts.Token);
            
            await Task.WhenAll(heartbeatTask, receiveTask);
        }

        private async Task SendHeartbeatLoop(CancellationToken ct)
        {
            if (_stream == null) return;

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var processes = GetRunningProcesses();
                    var msg = new StatusUpdateMessage
                    {
                        MessageType = MessageType.StatusUpdate,
                        SenderId = Environment.MachineName,
                        Psk = Protocol.PSK,
                        Status = ClientStatus.Normal,
                        Details = $"Running {processes} processes | Memory: {GetMemoryUsage()}MB"
                    };
                    var data = MessageSerializer.Serialize(msg);
                    await _stream.WriteAsync(data, 0, data.Length, ct);
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Heartbeat sent - {processes} processes, {GetMemoryUsage()}MB memory");
                    await Task.Delay(TimeSpan.FromSeconds(10), ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Send error: {ex.Message}");
                    break;
                }
            }
        }

        private int GetRunningProcesses()
        {
            try
            {
                return Process.GetProcesses().Length;
            }
            catch
            {
                return 0;
            }
        }

        private long GetMemoryUsage()
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();
                return currentProcess.WorkingSet64 / (1024 * 1024); // Convert to MB
            }
            catch
            {
                return 0;
            }
        }

        private async Task ReceiveCommandLoop(CancellationToken ct)
        {
            if (_stream == null) return;

            var buffer = new byte[4096];
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    // Read message length (4 bytes, big-endian)
                    int totalRead = 0;
                    while (totalRead < 4)
                    {
                        int read = await _stream.ReadAsync(buffer, totalRead, 4 - totalRead, ct);
                        if (read == 0) throw new EndOfStreamException("Connection closed by host");
                        totalRead += read;
                    }

                    int messageLength = BitConverter.ToInt32(buffer, 0);
                    if (!BitConverter.IsLittleEndian) messageLength = EndianSwap(messageLength);

                    // Read message body
                    totalRead = 0;
                    while (totalRead < messageLength)
                    {
                        int read = await _stream.ReadAsync(buffer, 4 + totalRead, messageLength - totalRead, ct);
                        if (read == 0) throw new EndOfStreamException("Connection closed by host");
                        totalRead += read;
                    }

                    // Deserialize and handle
                    var msg = MessageSerializer.DeserializeFromSpan(new ReadOnlySpan<byte>(buffer, 4, messageLength));
                    if (msg is CommandMessage cmd)
                    {
                        await HandleCommand(cmd);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (EndOfStreamException)
                {
                    Console.WriteLine("Host disconnected");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Receive error: {ex.Message}");
                    break;
                }
            }
        }

        private async Task HandleCommand(CommandMessage cmd)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Command received: {cmd.Command}");

            try
            {
                switch (cmd.Command)
                {
                    case CommandType.Warn:
                        await ExecuteWarn(cmd.Payload ?? "Warning from administrator");
                        break;

                    case CommandType.Screenshot:
                        await ExecuteScreenshot();
                        break;

                    case CommandType.Shutdown:
                        await ExecuteShutdown();
                        break;

                    default:
                        Console.WriteLine($"Unknown command: {cmd.Command}");
                        break;
                }

                // Send acknowledgement
                await SendAcknowledgement(cmd.Command.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Command execution error: {ex.Message}");
            }
        }

        private async Task ExecuteWarn(string message)
        {
            Console.WriteLine($"[WARNING] {message}");
            // Display message to user (platform-specific implementation)
            await Task.CompletedTask;
        }

        private async Task ExecuteScreenshot()
        {
            try
            {
                Console.WriteLine("[SCREENSHOT] Capturing screen...");
                
                // Create evidence directory if it doesn't exist
                string evidenceDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LabGuard", "Evidence");
                Directory.CreateDirectory(evidenceDir);

                string filename = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                string filepath = Path.Combine(evidenceDir, filename);

                // Capture screenshot
                using (var bitmap = new System.Drawing.Bitmap(
                    System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                    System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    using (var g = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Location, System.Drawing.Point.Empty, bitmap.Size);
                    }
                    bitmap.Save(filepath, System.Drawing.Imaging.ImageFormat.Png);
                }

                Console.WriteLine($"[SCREENSHOT] Saved to {filepath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SCREENSHOT] Error: {ex.Message}");
            }
            
            await Task.CompletedTask;
        }

        private async Task ExecuteShutdown()
        {
            Console.WriteLine("[SHUTDOWN] System will shut down in 60 seconds");
            // Start shutdown timer
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = "/s /t 60 /c \"Lab machine shutting down per administrator command\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            await Task.CompletedTask;
        }

        private async Task SendAcknowledgement(string commandType)
        {
            if (_stream == null) return;

            var ack = new BaseMessage
            {
                MessageType = MessageType.Acknowledgement,
                SenderId = Environment.MachineName,
                Psk = Protocol.PSK
            };

            try
            {
                var data = MessageSerializer.Serialize(ack);
                await _stream.WriteAsync(data, 0, data.Length);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Acknowledged: {commandType}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending acknowledgement: {ex.Message}");
            }
        }

        private static int EndianSwap(int value)
        {
            unchecked
            {
                uint uvalue = (uint)value;
                return (int)(((uvalue & 0xFF000000) >> 24) |
                             ((uvalue & 0x00FF0000) >> 8) |
                             ((uvalue & 0x0000FF00) << 8) |
                             ((uvalue & 0x000000FF) << 24));
            }
        }
    }
}
