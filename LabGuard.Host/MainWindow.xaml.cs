using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using LabGuard.Common;

namespace LabGuard.Host
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<ClientInfo> _clients = new();
        private HostListener? _listener;
        private Dictionary<string, ClientInfo> _clientMap = new();

        public MainWindow()
        {
            InitializeComponent();
            ClientList.ItemsSource = _clients;

            // Start logo animation
            AnimateLogo();

            // Start listener
            _listener = new HostListener();
            _listener.OnClientConnected += OnClientConnected;
            _listener.OnClientDisconnected += OnClientDisconnected;
            _listener.OnClientStatusUpdated += OnClientStatusUpdated;
            _listener.Start();
            StatusBlock.Text = "Listening for clients...";
        }

        private void OnTopologyClick(object sender, RoutedEventArgs e)
        {
            if (_listener == null) return;
            var win = new TopologyWindow(_listener);
            win.Owner = this;
            win.Show();
        }

        private void AnimateLogo()
        {
            // Create rotation animation for logo
            var rotationAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(3),
                RepeatBehavior = RepeatBehavior.Forever
            };
            
            LogoRotation.BeginAnimation(RotateTransform.AngleProperty, rotationAnimation);

            // Pulse animation for status indicator
            var pulseAnimation = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = RepeatBehavior.Forever,
                Duration = TimeSpan.FromSeconds(2)
            };
            pulseAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromPercent(0.0)));
            pulseAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0.5, KeyTime.FromPercent(0.5)));
            pulseAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromPercent(1.0)));

            StatusIndicator.BeginAnimation(OpacityProperty, pulseAnimation);
        }

        private void OnClientConnected(ClientInfo client)
        {
            Dispatcher.Invoke(() =>
            {
                if (!_clientMap.ContainsKey(client.Id))
                {
                    // Ensure we use the record with details if available, else default
                    var info = new ClientInfo(client.Id, client.Hostname, client.Status, client.Details);
                    _clients.Add(info);
                    _clientMap[client.Id] = info;
                    StatusBlock.Text = $"Client connected: {client.Hostname}";
                    Console.WriteLine($"Added client {client.Hostname} to UI");
                }
            });
        }

        private void OnClientDisconnected(string clientId)
        {
            Dispatcher.Invoke(() =>
            {
                if (_clientMap.TryGetValue(clientId, out var client))
                {
                    _clients.Remove(client);
                    _clientMap.Remove(clientId);
                    StatusBlock.Text = $"Client disconnected: {clientId}";
                }
            });
        }

        private void OnClientStatusUpdated(string clientId, string details)
        {
            Dispatcher.Invoke(() =>
            {
                if (_clientMap.TryGetValue(clientId, out var client))
                {
                    var index = _clients.IndexOf(client);
                    if (index >= 0)
                    {
                        var updated = new ClientInfo(client.Id, client.Hostname, client.Status, details);
                        _clients[index] = updated;
                        _clientMap[clientId] = updated;
                    }
                }
            });
        }

        private async void OnWarnClick(object sender, RoutedEventArgs e)
        {
            if (_listener == null) return;
            var target = ClientList.SelectedItem as ClientInfo;
            var targets = target != null ? new[] { target } : _clients.ToArray();

            if (targets.Length == 0) return;

            string msg = target != null 
                ? $"Send Warning to {target.Hostname}?" 
                : $"Send Warning to ALL {targets.Length} clients?";

            if (MessageBox.Show(msg, "Confirm Warning", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            var cmd = new CommandMessage
            {
                MessageType = MessageType.Command,
                SenderId = Environment.MachineName,
                Psk = Protocol.PSK,
                Command = CommandType.Warn,
                Payload = "ALERT: Unauthorized activity detected on your station. Please resume work."
            };

            foreach (var t in targets)
            {
                await _listener.SendCommandToClient(t.Id, cmd);
            }
            StatusBlock.Text = $"Warning sent to {targets.Length} clients";
        }

        private async void OnScreenshotClick(object sender, RoutedEventArgs e)
        {
            if (_listener == null) return;
            var target = ClientList.SelectedItem as ClientInfo;
            var targets = target != null ? new[] { target } : _clients.ToArray();

            if (targets.Length == 0) return;

            string msg = target != null 
                ? $"Take Screenshot of {target.Hostname}?" 
                : $"Take Screenshot of ALL {targets.Length} clients?";

            if (MessageBox.Show(msg, "Confirm Screenshot", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            var cmd = new CommandMessage
            {
                MessageType = MessageType.Command,
                SenderId = Environment.MachineName,
                Psk = Protocol.PSK,
                Command = CommandType.Screenshot,
                Payload = null
            };

            foreach (var t in targets)
            {
                await _listener.SendCommandToClient(t.Id, cmd);
            }
            StatusBlock.Text = $"Screenshot command sent to {targets.Length} clients";
        }

        private async void OnShutdownClick(object sender, RoutedEventArgs e)
        {
            if (_listener == null) return;
            var target = ClientList.SelectedItem as ClientInfo;
            var targets = target != null ? new[] { target } : _clients.ToArray();

            if (targets.Length == 0) return;

            string msg = target != null 
                ? $"SHUTDOWN {target.Hostname}?" 
                : $"SHUTDOWN ALL {targets.Length} clients? THIS IS DANGEROUS!";

            if (MessageBox.Show(msg, "Confirm Shutdown", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

            var cmd = new CommandMessage
            {
                MessageType = MessageType.Command,
                SenderId = Environment.MachineName,
                Psk = Protocol.PSK,
                Command = CommandType.Shutdown,
                Payload = null
            };

            foreach (var t in targets)
            {
                await _listener.SendCommandToClient(t.Id, cmd);
            }
            StatusBlock.Text = $"Shutdown command sent to {targets.Length} clients";
        }
    }
}
