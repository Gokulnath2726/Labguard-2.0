using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using LabGuard.Common;

namespace LabGuard.Host
{
    public partial class TopologyView : UserControl
    {
        private HostListener? _listener;
        private readonly List<Border> _nodes = new();
        private readonly List<TextBlock> _labels = new();
        private readonly List<string?> _mappedClientIds = new();
        private DispatcherTimer? _timer;
        private string _searchText = string.Empty;
        private bool _misuseOnly = false;

        public TopologyView()
        {
            InitializeComponent();
            BuildLabLayout();
        }

        public void Initialize(HostListener listener)
        {
            _listener = listener;
            StartRefreshTimer();
        }

        private void StartRefreshTimer()
        {
            if (_timer != null) return;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _timer.Tick += (s, e) => RefreshStatuses();
            _timer.Start();
            RefreshStatuses();
        }

        private void OnSimulationToggle(object sender, RoutedEventArgs e)
        {
            RefreshStatuses();
        }

        private void BuildLabLayout()
        {
            BusCanvas.Children.Clear();
            _nodes.Clear();
            _labels.Clear();
            _mappedClientIds.Clear();

            // Lab Heading
            var heading = new TextBlock
            {
                Text = "Lab Hub 1 - Computer Lab View (70 PCs)",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            
            BusCanvas.Children.Add(heading);

            // Layout Configuration
            int rows = 7;
            int cols = 10;
            
            double startX = 50;
            double startY = 80;
            
            double boxWidth = 80;
            double boxHeight = 50;
            double hSpacing = 15;
            double vSpacing = 20;

            // Update Canvas Size
            double totalWidth = startX + (cols * (boxWidth + hSpacing)) + 50;
            double totalHeight = startY + (rows * (boxHeight + vSpacing)) + 50;
            BusCanvas.Width = Math.Max(1000, totalWidth);
            BusCanvas.Height = Math.Max(700, totalHeight);

            // Center heading
            Canvas.SetLeft(heading, (BusCanvas.Width - 450) / 2);
            Canvas.SetTop(heading, 20);

            // No bus lines, just grid
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int nodeIndex = (r * cols) + c;
                    
                    double x = startX + (c * (boxWidth + hSpacing));
                    double y = startY + (r * (boxHeight + vSpacing));

                    // PC Node (Border acting as Card)
                    var border = new Border
                    {
                        Width = boxWidth,
                        Height = boxHeight,
                        Background = new SolidColorBrush(Color.FromRgb(107, 114, 128)), // Gray default
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(3),
                        Tag = nodeIndex,
                        Cursor = System.Windows.Input.Cursors.Hand
                    };
                    
                    // Mouse events
                    border.MouseLeftButtonUp += Node_Click;

                    _mappedClientIds.Add(null); 
                    _nodes.Add(border);

                    Canvas.SetLeft(border, x);
                    Canvas.SetTop(border, y);
                    BusCanvas.Children.Add(border);

                    // Label inside the box
                    var lbl = new TextBlock
                    {
                        Text = $"PC-{nodeIndex + 1}",
                        FontSize = 12,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = Brushes.Black,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        IsHitTestVisible = false // Click on border
                    };
                    
                    border.Child = lbl;
                    _labels.Add(lbl);
                }
            }
            
            // Draw Legend
            DrawLegend(10, 10);
        }

        private void DrawLegend(double x, double y)
        {
            // Optional: Legend is implicit in colors, avoiding clutter as per image style
        }

        private void Node_Click(object? sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border b && b.Tag is int idx)
            {
                var clientId = _mappedClientIds[idx];
                
                var menu = new ContextMenu();
                var headerItem = new MenuItem { Header = $"PC-{idx+1} Options", IsEnabled = false, FontWeight = FontWeights.Bold };
                menu.Items.Add(headerItem);
                menu.Items.Add(new Separator());

                var miAlert = new MenuItem { Header = "📢  Send Alert Notification" };
                miAlert.Click += (_, __) => SendAlertToNode(idx);
                menu.Items.Add(miAlert);

                var miShutdown = new MenuItem { Header = "⏻  Shutdown PC" };
                miShutdown.Click += (_, __) => ShutdownNode(idx);
                menu.Items.Add(miShutdown);

                var miScreenshot = new MenuItem { Header = "📸  Take Screenshot" };
                miScreenshot.Click += (_, __) => ScreenshotNode(idx);
                menu.Items.Add(miScreenshot);

                var miScreenshotShutdown = new MenuItem
                {
                    Header = "📸  Take Screenshot & Shutdown",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = System.Windows.Media.Brushes.OrangeRed
                };
                miScreenshotShutdown.Click += (_, __) => ScreenshotAndShutdownNode(idx);
                menu.Items.Add(miScreenshotShutdown);

                if (b.ToolTip != null)
                {
                     var tooltip = b.ToolTip.ToString();
                     var miDetails = new MenuItem { Header = "🔍  View Activity Details" };
                     miDetails.Click += (_, __) => MessageBox.Show($"Current Activity:\n{tooltip}", "Activity Log");
                     menu.Items.Add(miDetails);
                }

                b.ContextMenu = menu;
                b.ContextMenu.IsOpen = true;
            }
        }

        private void SendAlertToNode(int idx)
        {
            var clientId = _mappedClientIds[idx];
            bool isSimulation = (clientId != null && clientId.StartsWith("SIM-PC"));

            if (clientId == null)
            {
                MessageBox.Show("PC is unmapped.", "Topology Command Error");
                return;
            }

            if (isSimulation)
            {
                 MessageBox.Show($"[SIMULATION] Alert sent to {clientId} (PC-{idx+1}).\nHost: Please stop unauthorized activity.", "Notification Sent");
                 return;
            }

            if (_listener == null)
            {
               MessageBox.Show("Listener not active.", "Error");
               return;
            }

            var cmd = new CommandMessage
            {
                MessageType = MessageType.Command,
                SenderId = Environment.MachineName,
                Psk = Protocol.PSK,
                Command = CommandType.Warn,
                Payload = "ALERT: Unauthorized activity detected on your station. Please resume work."
            };
            _ = _listener.SendCommandToClient(clientId, cmd);
            MessageBox.Show($"Alert sent to PC-{idx+1} ({clientId})", "Notification Sent");
        }

        private void ShutdownNode(int idx)
        {
            var clientId = _mappedClientIds[idx];
            bool isSimulation = (clientId != null && clientId.StartsWith("SIM-PC"));

            if (clientId == null)
            {
                MessageBox.Show("PC is unmapped.", "Topology Command Error");
                return;
            }

            if (MessageBox.Show($"Are you sure you want to SHUTDOWN PC-{idx+1} ({clientId})?", "Confirm Shutdown", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (isSimulation)
                {
                     MessageBox.Show($"[SIMULATION] Shutdown command dispatched to {clientId}.", "Shutdown Initiated");
                     return;
                }

                if (_listener == null) return;

                var cmd = new CommandMessage
                {
                    MessageType = MessageType.Command,
                    SenderId = Environment.MachineName,
                    Psk = Protocol.PSK,
                    Command = CommandType.Shutdown,
                    Payload = null
                };
                _ = _listener.SendCommandToClient(clientId, cmd);
                MessageBox.Show($"Shutdown command dispatched to PC-{idx+1}", "Shutdown Initiated");
            }
        }

        private void ScreenshotNode(int idx)
        {
            var clientId = _mappedClientIds[idx];
            bool isSimulation = (clientId != null && clientId.StartsWith("SIM-PC"));

            if (clientId == null)
            {
                MessageBox.Show("PC is unmapped.", "Topology Command Error");
                return;
            }

            if (isSimulation)
            {
                MessageBox.Show($"[SIMULATION] Screenshot captured on {clientId}.", "Screenshot");
                return;
            }

            if (_listener == null) return;

            var cmd = new CommandMessage
            {
                MessageType = MessageType.Command,
                SenderId = Environment.MachineName,
                Psk = Protocol.PSK,
                Command = CommandType.Screenshot,
                Payload = null
            };
            _ = _listener.SendCommandToClient(clientId, cmd);
            MessageBox.Show($"Screenshot command sent to PC-{idx + 1} ({clientId}).\nThe image will be saved on the client machine.", "Screenshot Sent", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ScreenshotAndShutdownNode(int idx)
        {
            var clientId = _mappedClientIds[idx];
            bool isSimulation = (clientId != null && clientId.StartsWith("SIM-PC"));

            if (clientId == null)
            {
                MessageBox.Show("PC is unmapped.", "Topology Command Error");
                return;
            }

            if (MessageBox.Show(
                    $"Take a screenshot then SHUTDOWN PC-{idx + 1} ({clientId})?\n\nThe screenshot will be saved on the client machine before shutdown.",
                    "Confirm Screenshot & Shutdown",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            if (isSimulation)
            {
                MessageBox.Show(
                    $"[SIMULATION] Screenshot captured on {clientId}.\nShutdown command dispatched 3 seconds later.",
                    "Screenshot & Shutdown Initiated");
                return;
            }

            if (_listener == null) return;

            // Step 1 – Screenshot
            var screenshotCmd = new CommandMessage
            {
                MessageType = MessageType.Command,
                SenderId = Environment.MachineName,
                Psk = Protocol.PSK,
                Command = CommandType.Screenshot,
                Payload = null
            };
            _ = _listener.SendCommandToClient(clientId, screenshotCmd);

            // Step 2 – Shutdown after 3 s delay so the screenshot finishes saving
            _ = Task.Delay(3000).ContinueWith(_ =>
            {
                var shutdownCmd = new CommandMessage
                {
                    MessageType = MessageType.Command,
                    SenderId = Environment.MachineName,
                    Psk = Protocol.PSK,
                    Command = CommandType.Shutdown,
                    Payload = null
                };
                _ = _listener.SendCommandToClient(clientId!, shutdownCmd);
            });

            MessageBox.Show(
                $"Screenshot command sent to PC-{idx + 1}.\nShutdown will follow in 3 seconds.",
                "Screenshot & Shutdown Initiated",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void RefreshStatuses()
        {
            bool simulate = false;
            if (this.FindName("SimulationToggle") is CheckBox cb && cb.IsChecked == true)
            {
                simulate = true;
            }

            if (_listener == null && !simulate) return;
            var sessions = (!simulate && _listener != null) ? _listener.GetConnectedClients().ToList() : new List<ClientSession>();

            int nodes = _nodes.Count;

            // Prepare state maps
            var clientIdByIndex = new string?[nodes];
            var hostnameByIndex = new string[nodes];
            var statusByIndex = new ClientStatus[nodes];
            var detailsByIndex = new string[nodes];

            for (int i = 0; i < nodes; i++)
            {
                clientIdByIndex[i] = null;
                hostnameByIndex[i] = $"PC-{i + 1}";
                statusByIndex[i] = ClientStatus.Offline;
                detailsByIndex[i] = string.Empty;
            }

            if (simulate)
            {
                var random = new Random();
                for (int i = 0; i < nodes; i++)
                {
                    if (random.NextDouble() > 0.3) // 70% Online
                    {
                        clientIdByIndex[i] = $"SIM-PC-{i + 1:D2}";
                        hostnameByIndex[i] = $"PC-{i + 1}";
                        
                        // 15% Misuse
                        if (random.NextDouble() < 0.15)
                        {
                            statusByIndex[i] = ClientStatus.Misuse;
                            var activities = new[] { "Playing Valorant", "Running Steam", "Browsing Restricted Site", "BitTorrent Client" };
                            detailsByIndex[i] = activities[random.Next(activities.Length)];
                        }
                        else
                        {
                            statusByIndex[i] = ClientStatus.Normal;
                            detailsByIndex[i] = "Authorized Work";
                        }
                    }
                }
            }
            else
            {
                // Real mapping logic
                bool[] occupied = new bool[nodes];
                foreach (var sess in sessions)
                {
                    int targetIndex = -1;
                    // Try to parse "PC-5" or "5" from hostname
                    var m = Regex.Match(sess.Hostname ?? string.Empty, "(\\d+)(?!.*\\d)");
                    if (m.Success && int.TryParse(m.Value, out var num))
                    {
                        if (num >= 1 && num <= nodes && !occupied[num - 1])
                        {
                            targetIndex = num - 1;
                        }
                    }

                    if (targetIndex == -1)
                    {
                        // Find first free
                        for (int i = 0; i < nodes; i++) if (!occupied[i]) { targetIndex = i; break; }
                    }

                    if (targetIndex != -1)
                    {
                        occupied[targetIndex] = true;
                        clientIdByIndex[targetIndex] = sess.Id;
                        hostnameByIndex[targetIndex] = sess.Hostname ?? sess.Id;
                        statusByIndex[targetIndex] = sess.Status;
                        detailsByIndex[targetIndex] = sess.Status == ClientStatus.Misuse ? (sess.Hostname + " (Unauthorized Activity)") : "Authorized Work";
                    }
                }
            }

            // Apply to UI
            for (int i = 0; i < nodes; i++)
            {
                _mappedClientIds[i] = clientIdByIndex[i];
                ApplyNodeState(i, clientIdByIndex[i], hostnameByIndex[i], statusByIndex[i], detailsByIndex[i]);
            }
        }

        private void ApplyNodeState(int i, string? clientId, string hostname, ClientStatus status, string details)
        {
            // Colors from Image
            // Green: #00C853 (Vivid Green) or #22C55E
            // Red: #EF4444 (Red)
            // Gray: #6B7280
            
            Brush fill;
            switch (status)
            {
                case ClientStatus.Normal:
                    fill = new SolidColorBrush(Color.FromRgb(34, 197, 94)); // Green
                    break;
                case ClientStatus.Misuse:
                    fill = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
                    break;
                default:
                    fill = new SolidColorBrush(Color.FromRgb(100, 100, 100)); // Dark Gray for offline
                    break;
            }

            _nodes[i].Background = fill;

            // Tooltip
            if (status != ClientStatus.Offline && clientId != null)
            {
                _nodes[i].ToolTip = $"ID: {clientId}\nHost: {hostname}\nStatus: {status}\nDetails: {details}";
            }
            else
            {
                _nodes[i].ToolTip = $"PC-{i + 1} (Offline)";
            }
            
            string labelText = $"PC-{i + 1}";
            if (hostname != null && hostname.StartsWith("PC-")) 
                labelText = hostname; // use hostname if matches pattern, else sticky label logic?
            // Actually image shows static PC-1...PC-70. Let's keep index based names in box.
            _labels[i].Text = $"PC-{i + 1}";


            // Search
            bool matchesSearch = true;
            if (!string.IsNullOrEmpty(_searchText))
            {
                var s = _searchText.ToLowerInvariant();
                matchesSearch = (hostname ?? "").ToLowerInvariant().Contains(s);
            }

            // Misuse Filter
            if (_misuseOnly && status != ClientStatus.Misuse)
            {
                _nodes[i].Opacity = 0.1;
                _labels[i].Opacity = 0.1;
            }
            else if (!matchesSearch)
            {
                _nodes[i].Opacity = 0.2;
                _labels[i].Opacity = 0.2;
            }
            else
            {
                _nodes[i].Opacity = 1.0;
                _labels[i].Opacity = 1.0;
            }
        }

        private void OnSearchChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb) _searchText = tb.Text?.Trim() ?? string.Empty;
            RefreshStatuses();
        }

        private void OnFilterChanged(object sender, RoutedEventArgs e)
        {
            if (this.FindName("MisuseOnlyFilter") is CheckBox cb) _misuseOnly = cb.IsChecked == true;
            RefreshStatuses();
        }
    }
}
