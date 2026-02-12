# LabGuard - Developer Quick Reference Guide

## Quick Links

| Document | Purpose |
|----------|---------|
| [README.md](README.md) | User-friendly overview & quick start |
| [IMPLEMENTATION.md](IMPLEMENTATION.md) | Full technical architecture & design |
| [DELIVERY_SUMMARY.md](DELIVERY_SUMMARY.md) | Project completion status |
| [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) | File organization |
| [SETUP.md](SETUP.md) | Installation & configuration |

## Code Organization

### LabGuard.Common (Shared Library)
**Location**: `LabGuard.Common/`

#### Protocol.cs
```csharp
// Configuration constants
public static class Protocol
{
    public const string PSK = "CHANGE_ME_PSK_12345";  // Pre-Shared Key
}

// Enums & data types
public enum ClientStatus { Normal, Misuse, Offline }
public record ClientInfo(string Id, string Hostname, ClientStatus Status);
```

#### Messages.cs
```csharp
// Message types sent between client and host
public enum MessageType { StatusUpdate, Command, Acknowledgement }
public enum CommandType { Warn, Screenshot, Shutdown }

// Base class for all messages
public class BaseMessage { ... }

// Derived message types
public class StatusUpdateMessage : BaseMessage { }
public class CommandMessage : BaseMessage { }

// Serialization helper
public static class MessageSerializer
{
    public static byte[] Serialize(BaseMessage msg);
    public static BaseMessage? DeserializeFromSpan(ReadOnlySpan<byte> span);
}
```

### LabGuard.Host (Monitoring Console)

**Location**: `LabGuard.Host/`

#### MainWindow.xaml.cs (UI Logic)
```csharp
// Key Methods:
public partial class MainWindow : Window
{
    private ObservableCollection<ClientInfo> _clients;
    private HostListener? _listener;
    
    // Called when client connects
    private void OnClientConnected(ClientInfo client)
    
    // Called when client disconnects  
    private void OnClientDisconnected(string clientId)
    
    // Called when client status updates
    private void OnClientStatusUpdated(string clientId, string details)
    
    // UI Event Handlers
    private void OnWarnClick(object sender, RoutedEventArgs e)
    private void OnScreenshotClick(object sender, RoutedEventArgs e)
    private void OnShutdownClick(object sender, RoutedEventArgs e)
}
```

#### HostListener.cs (TCP Server)
```csharp
// Client session representation
public class ClientSession
{
    public string Id { get; set; }
    public string Hostname { get; set; }
    public TcpClient TcpClient { get; set; }
    public NetworkStream Stream { get; set; }
    public DateTime LastHeartbeat { get; set; }
    public ClientStatus Status { get; set; }
}

// Main listener class
public class HostListener
{
    // Initialize with custom port: new HostListener(9000)
    
    public void Start();  // Begin listening for connections
    
    public IEnumerable<ClientSession> GetConnectedClients();
    
    public Task SendCommandToClient(string clientId, CommandMessage command);
    
    // Events
    public event Action<ClientInfo>? OnClientConnected;
    public event Action<string>? OnClientDisconnected;
    public event Action<string, string>? OnClientStatusUpdated;
}
```

### LabGuard.Client (Agent Software)

**Location**: `LabGuard.Client/`

#### Program.cs (Entry Point)
```csharp
static async Task Main(string[] args)
{
    var client = new NetworkClient("127.0.0.1", 9000);
    try
    {
        await client.StartAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to start: {ex.Message}");
    }
}
```

#### NetworkClient.cs (TCP Client Agent)
```csharp
public class NetworkClient
{
    // Constructor: specify host and port
    public NetworkClient(string host = "127.0.0.1", int port = 9000)
    
    public async Task StartAsync();  // Connect and start both loops
    
    // Private Background Loops
    private async Task SendHeartbeatLoop(CancellationToken ct);
    private async Task ReceiveCommandLoop(CancellationToken ct);
    
    // Command Handlers
    private async Task HandleCommand(CommandMessage cmd);
    private async Task ExecuteWarn(string message);
    private async Task ExecuteScreenshot();
    private async Task ExecuteShutdown();
    
    // Helper Methods
    private async Task SendAcknowledgement(string commandType);
    private int GetRunningProcesses();
    private long GetMemoryUsage();
}
```

## Common Development Tasks

### 1. Add New Command Type

**Step 1**: Update `LabGuard.Common/Messages.cs`
```csharp
public enum CommandType 
{ 
    Warn, 
    Screenshot, 
    Shutdown,
    YourNewCommand  // 👈 Add here
}
```

**Step 2**: Update `LabGuard.Client/NetworkClient.cs`
```csharp
switch (cmd.Command)
{
    case CommandType.YourNewCommand:
        await ExecuteYourNewCommand(cmd.Payload);
        break;
    // ...
}

private async Task ExecuteYourNewCommand(string? payload)
{
    // Your implementation
}
```

**Step 3**: Update `LabGuard.Host/MainWindow.xaml.cs`
```csharp
// Add button click handler
private void OnYourCommandClick(object sender, RoutedEventArgs e)
{
    if (ClientList.SelectedItem is ClientInfo client && _listener != null)
    {
        var cmd = new CommandMessage
        {
            MessageType = MessageType.Command,
            SenderId = Environment.MachineName,
            Psk = Protocol.PSK,
            Command = CommandType.YourNewCommand,
            Payload = "optional data"
        };
        _ = _listener.SendCommandToClient(client.Id, cmd);
    }
}
```

**Step 4**: Update `LabGuard.Host/MainWindow.xaml` (Add button)
```xml
<Button Name="YourCommandButton" 
        Click="OnYourCommandClick"
        Content="Your Command" />
```

### 2. Modify Heartbeat Frequency

**File**: `LabGuard.Client/NetworkClient.cs`
```csharp
// LineNumber ~60 - Look for Task.Delay
await Task.Delay(TimeSpan.FromSeconds(10));  // Change 10 to your value
```

### 3. Change Server Port

**File**: `LabGuard.Host/HostListener.cs`
```csharp
public HostListener(int port = 9000)  // Change 9000
```

**File**: `LabGuard.Client/NetworkClient.cs`
```csharp
public NetworkClient(string host = "127.0.0.1", int port = 9000)  // Change 9000
```

### 4. Change Connection Address (Client)

**File**: `LabGuard.Client/Program.cs`
```csharp
var client = new NetworkClient("192.168.1.100", 9000);  // Your server IP
```

### 5. Save Data in Heartbeat

**File**: `LabGuard.Client/NetworkClient.cs` - `SendHeartbeatLoop()` method
```csharp
var msg = new StatusUpdateMessage
{
    MessageType = MessageType.StatusUpdate,
    SenderId = Environment.MachineName,
    Psk = Protocol.PSK,
    Status = ClientStatus.Normal,
    Details = $"Your custom data here: {GetYourData()}"
};
```

### 6. Access All Connected Clients

**File**: `LabGuard.Host/MainWindow.xaml.cs` or elsewhere
```csharp
var allClients = _listener?.GetConnectedClients();
foreach (var session in allClients ?? Enumerable.Empty<ClientSession>())
{
    Console.WriteLine($"Client: {session.Id} - {session.Status}");
}
```

### 7. Send Command Programmatically

**File**: Any file with access to HostListener
```csharp
var cmd = new CommandMessage
{
    MessageType = MessageType.Command,
    SenderId = "Host",
    Psk = Protocol.PSK,
    Command = CommandType.Screenshot,
    Payload = null
};

await _listener.SendCommandToClient("client-id-here", cmd);
```

## Debugging Tips

### Enable Debug Output
All classes use `Console.WriteLine()` for logging.
To see real-time output:
```bash
dotnet run --project LabGuard.Host | tee host.log
dotnet run --project LabGuard.Client | tee client.log
```

### Check Client Connection
In Host:
```csharp
var connected = _listener.GetConnectedClients().Count();
Console.WriteLine($"Connected clients: {connected}");
```

### Monitor Message Flow
Look for these console messages:
```
[Host] "[HH:mm:ss] Status from LAB-PC-01: Normal - Running 187 processes"
[Client] "[HH:mm:ss] Heartbeat sent - 187 processes, 4567MB memory"
[Host] "[HH:mm:ss] Client LAB-PC-01 connected"
[Client] "[HH:mm:ss] Command received: Screenshot"
```

## Thread Safety Considerations

### HostListener
- Uses `lock (_clients)` for dictionary access
- Each client runs on separate thread
- UI updates via `Dispatcher.Invoke()`

### NetworkClient
- Uses `CancellationToken` for graceful shutdown
- Separate tasks for send/receive loops
- `lock` protection for session state

### MainWindow
- All UI updates on main thread via `Dispatcher`
- Never block UI thread for I/O

## Performance Optimization Ideas

1. **Connection Pooling**: Reuse TCP connections
2. **Message Batching**: Send multiple commands in one packet
3. **Compression**: Gzip large messages before transmission
4. **Caching**: Cache client status locally
5. **Async All The Way**: Use async/await throughout

## Testing Checklist

- [ ] Build without errors: `dotnet build`
- [ ] Release build: `dotnet build -c Release`
- [ ] Localhost test: Run Host & Client on same machine
- [ ] LAN test: Run on different machines
- [ ] Multiple clients: Run 3+ clients simultaneously
- [ ] Command execution: Test each button
- [ ] Error handling: Disconnect client and observe recovery
- [ ] Timeout behavior: Stop client heartbeat and check timeout

## Building & Publishing

### Development Build
```bash
dotnet build
```

### Release Build
```bash
dotnet build -c Release
```

### Publish as Standalone
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

### Output Location
```
LabGuard.Host/bin/Release/net8.0-windows/LabGuard.Host.exe
LabGuard.Client/bin/Release/net8.0-windows/LabGuard.Client.exe
```

## Key Files to Modify for Different Features

| Feature | File | Method |
|---------|------|--------|
| New command | Messages.cs, NetworkClient.cs | ExecuteNewCommand() |
| UI button | MainWindow.xaml(.cs) | OnNewClick() |
| Heartbeat frequency | NetworkClient.cs | SendHeartbeatLoop() |
| Screenshot location | NetworkClient.cs | ExecuteScreenshot() |
| Server port | HostListener.cs | Constructor |
| Security key | Protocol.cs | PSK constant |
| Client timeout | HostListener.cs | MonitorClientHealth() |

## Useful Code Snippets

### Send Acknowledgement
```csharp
await SendAcknowledgement("CommandType");
```

### Get Current Timestamp
```csharp
DateTime.UtcNow  // Use UTC for consistency
```

### Lock Dictionary Access
```csharp
lock (_clients)
{
    _clients.TryGetValue(clientId, out var session);
}
```

### Safe String Null Check
```csharp
cmd.Payload ?? "default value"
```

### Fire and Forget Task
```csharp
_ = BackgroundTask();  // Suppress CS4014 warning
```

## Resources

- [Microsoft .NET 8 Documentation](https://docs.microsoft.com/dotnet/)
- [C# Language Reference](https://docs.microsoft.com/dotnet/csharp/)
- [WPF Documentation](https://docs.microsoft.com/dotnet/desktop/wpf/)
- [Async/Await Patterns](https://docs.microsoft.com/dotnet/csharp/asynchronous-programming/)
- [TCP/IP Sockets](https://docs.microsoft.com/dotnet/api/system.net.sockets)

## Getting Help

1. Check console output for error messages
2. Review corresponding implementation in IMPLEMENTATION.md
3. Look for similar example in existing code
4. Check TEST_SCENARIOS in README.md
5. Enable Debug output and trace execution

---

**Last Updated**: February 10, 2026  
**For Questions**: Refer to IMPLEMENTATION.md for detailed architecture

