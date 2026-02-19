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
                    _clients.Add(client);
                    _clientMap[client.Id] = client;
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
                        var updated = new ClientInfo(client.Id, client.Hostname, client.Status);
                        _clients[index] = updated;
                        _clientMap[clientId] = updated;
                    }
                }
            });
        }

        private void OnWarnClick(object sender, RoutedEventArgs e)
        {
            if (ClientList.SelectedItem is ClientInfo client && _listener != null)
            {
                var cmd = new CommandMessage
                {
                    MessageType = MessageType.Command,
                    SenderId = Environment.MachineName,
                    Psk = Protocol.PSK,
                    Command = CommandType.Warn,
                    Payload = "This system is being monitored. Unauthorized use is prohibited."
                };
                _ = _listener.SendCommandToClient(client.Id, cmd);
                MessageBox.Show($"Warning sent to {client.Hostname}", "LabGuard");
            }
        }

        private void OnScreenshotClick(object sender, RoutedEventArgs e)
        {
            if (ClientList.SelectedItem is ClientInfo client && _listener != null)
            {
                var cmd = new CommandMessage
                {
                    MessageType = MessageType.Command,
                    SenderId = Environment.MachineName,
                    Psk = Protocol.PSK,
                    Command = CommandType.Screenshot,
                    Payload = null
                };
                _ = _listener.SendCommandToClient(client.Id, cmd);
                MessageBox.Show($"Screenshot command sent to {client.Hostname}", "LabGuard");
            }
        }

        private void OnShutdownClick(object sender, RoutedEventArgs e)
        {
            if (ClientList.SelectedItem is ClientInfo client && _listener != null)
            {
                if (MessageBox.Show($"Shutdown {client.Hostname}?", "LabGuard", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var cmd = new CommandMessage
                    {
                        MessageType = MessageType.Command,
                        SenderId = Environment.MachineName,
                        Psk = Protocol.PSK,
                        Command = CommandType.Shutdown,
                        Payload = null
                    };
                    _ = _listener.SendCommandToClient(client.Id, cmd);
                    MessageBox.Show($"Shutdown command sent to {client.Hostname}", "LabGuard");
                }
            }
        }
    }
}
