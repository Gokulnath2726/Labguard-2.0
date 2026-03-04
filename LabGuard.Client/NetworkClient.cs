using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using LabGuard.Common;

namespace LabGuard.Client
{
    public class NetworkClient
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return "Unknown";
        }
        private readonly string _host;
        private readonly int _port;
        private TcpClient? _tcp;
        private NetworkStream? _stream;
        private CancellationTokenSource? _cts;

        private static readonly string[] BlockedProcesses = new string[]
        {
            "steam", "epicgameslauncher", "battle.net", "riotclientorigin",
            "robloxplayerbeta", "valorant", "discord", "spotify", "tor",
            "minecraft", "javaw"
        };

        private static readonly string[] BlockedKeywords = new string[]
        {
            "roblox", "twitch", "tiktok", "instagram", "facebook", "netflix",
            "poki", "discord", "game", "valorant", "spotify", "steam"
        };

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
                    var activeWindowTitle = GetActiveWindowTitle();
                    
                    ClientStatus currentStatus = ClientStatus.Normal;
                    string detailsStr = $"Running {processes} processes | App: {activeWindowTitle} | Memory: {GetMemoryUsage()}MB";

                    // Check for misuse
                    var runningProcessNames = Process.GetProcesses().Select(p => p.ProcessName.ToLower());
                    if (BlockedProcesses.Any(bp => runningProcessNames.Contains(bp)))
                    {
                        currentStatus = ClientStatus.Misuse;
                        detailsStr = $"[MISUSE DETECTED: BLOCKED APP RUNNING] " + detailsStr;
                    }
                    else if (BlockedKeywords.Any(bk => activeWindowTitle.ToLower().Contains(bk)))
                    {
                        currentStatus = ClientStatus.Misuse;
                        detailsStr = $"[MISUSE DETECTED: BLOCKED WINDOW KEYWORD] " + detailsStr;
                    }

                    var msg = new StatusUpdateMessage
                    {
                        MessageType = MessageType.StatusUpdate,
                        SenderId = Environment.MachineName,
                        Psk = Protocol.PSK,
                        Status = currentStatus,
                        Details = detailsStr
                    };
                    var data = MessageSerializer.Serialize(msg);
                    await _stream.WriteAsync(data, 0, data.Length, ct);
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Heartbeat sent - Status: {currentStatus}, Details: {detailsStr}");
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
            // Display message to user (red popup)
            Task.Run(() => {
                var form = new Form();
                form.Text = "WARNING - LabGuard";
                form.BackColor = Color.Red;
                form.ForeColor = Color.White;
                form.Size = new Size(500, 250);
                form.StartPosition = FormStartPosition.CenterScreen;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.TopMost = true;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                var label = new Label();
                label.Text = message;
                label.Font = new Font("Segoe UI", 16, FontStyle.Bold);
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.Dock = DockStyle.Fill;
                
                var btn = new Button();
                btn.Text = "Dismiss";
                btn.ForeColor = Color.Black;
                btn.BackColor = Color.White;
                btn.Dock = DockStyle.Bottom;
                btn.Height = 40;
                btn.Click += (s, e) => form.Close();

                form.Controls.Add(label);
                form.Controls.Add(btn);

                form.ShowDialog();
            });
            await Task.CompletedTask;
        }

        // Win32 P/Invoke for reliable screen capture
        [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);
        [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        [DllImport("gdi32.dll")] private static extern bool BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
        [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr hDC);
        [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);
        [DllImport("user32.dll")] private static extern int GetSystemMetrics(int nIndex);
        private const int SRCCOPY = 0x00CC0020;
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        private async Task ExecuteScreenshot()
        {
            try
            {
                Console.WriteLine("[SCREENSHOT] Capturing screen...");

                string evidenceDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "LabGuard", "Evidence");
                Directory.CreateDirectory(evidenceDir);

                string filename = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                string filepath = Path.Combine(evidenceDir, filename);

                Exception? captureError = null;
                var thread = new Thread(() =>
                {
                    IntPtr desktopWnd = IntPtr.Zero;
                    IntPtr desktopDC = IntPtr.Zero;
                    IntPtr memDC = IntPtr.Zero;
                    IntPtr hBitmap = IntPtr.Zero;
                    IntPtr oldBitmap = IntPtr.Zero;
                    try
                    {
                        int width  = GetSystemMetrics(SM_CXSCREEN);
                        int height = GetSystemMetrics(SM_CYSCREEN);

                        desktopWnd = GetDesktopWindow();
                        desktopDC  = GetDC(desktopWnd);
                        memDC      = CreateCompatibleDC(desktopDC);
                        hBitmap    = CreateCompatibleBitmap(desktopDC, width, height);
                        oldBitmap  = SelectObject(memDC, hBitmap);

                        BitBlt(memDC, 0, 0, width, height, desktopDC, 0, 0, SRCCOPY);

                        // Convert the HBITMAP to a managed Bitmap and save
                        using var bmp = System.Drawing.Image.FromHbitmap(hBitmap);
                        bmp.Save(filepath, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    catch (Exception ex)
                    {
                        captureError = ex;
                    }
                    finally
                    {
                        if (oldBitmap != IntPtr.Zero) SelectObject(memDC, oldBitmap);
                        if (hBitmap   != IntPtr.Zero) DeleteObject(hBitmap);
                        if (memDC     != IntPtr.Zero) DeleteDC(memDC);
                        if (desktopDC != IntPtr.Zero) ReleaseDC(desktopWnd, desktopDC);
                    }
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();

                if (captureError != null)
                    Console.WriteLine($"[SCREENSHOT] Capture failed: {captureError.Message}");
                else
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
