# LabGuard - Intelligent Laboratory Monitoring System

A centralized monitoring system for managing student computer labs using a client-server architecture over LAN.

## Projects

- **LabGuard.Common** – Shared protocol, messages, and types
- **LabGuard.Host** – WPF desktop application for staff monitoring and control
- **LabGuard.Client** – Background agent running on student PCs

## Tech Stack

- .NET 8.0
- WPF (Windows Presentation Foundation)
- TCP/IP networking with JSON serialization
- PSK authentication

## Quick Start

### Prerequisites
- .NET 8 SDK
- Windows OS

### Build
```bash
cd c:\23g133\Project\Labguard
dotnet build
```

### Run Host (Staff Console)
```bash
dotnet run --project LabGuard.Host
```

### Run Client (Agent on Student PC)
```bash
dotnet run --project LabGuard.Client
```

## Features (Under Development)

- ✓ Basic project structure
- ✓ Message protocol (JSON over TCP)
- ✓ Host WPF UI with client list
- ✓ Client heartbeat and connection
- ✓ Application monitoring (running processes)
- ✓ Remote commands (warning, screenshot, shutdown)
- ✓ Screenshot capture and storage
- ✓ Process/memory monitoring per client
- ⏳ Browser activity tracking
- ⏳ Windows Service installation
- ⏳ Evidence/screenshot storage UI viewer
- ⏳ Logging and reporting

## Testing the System

### Single-Machine Testing (Localhost)

1. **Terminal 1: Start the Host**
   ```bash
   dotnet run --project LabGuard.Host
   ```
   The WPF window opens and waits for client connections on port 9000.

2. **Terminal 2: Start a Client**
   ```bash
   dotnet run --project LabGuard.Client
   ```
   The client connects to localhost:9000 and sends heartbeats every 10 seconds.

3. **Interact via Host UI**
   - Select a connected client from the list
   - Click **Warn** to send a warning message
   - Click **Screenshot** to capture the client's screen (saved to `%LOCALAPPDATA%\LabGuard\Evidence`)
   - Click **Shutdown** to initiate system shutdown (60-second countdown)

### Multi-Machine Testing

1. Run **LabGuard.Host** on the monitor machine
2. Run **LabGuard.Client** on each lab machine, changing the server IP:
   ```csharp
   var client = new NetworkClient("192.168.X.X") // Server IP instead of 127.0.0.1
   ```

## Security Notes

- Change `Protocol.PSK` in `LabGuard.Common\Protocol.cs` before production
- Implement TLS for encrypted communication (currently using PSK placeholder)
- Restrict port 9000 to LAN only

## Next Steps

1. Implement application/process monitoring
2. Add Windows Service wrapper for Client
3. Implement remote command execution
4. Add screenshot capture and storage
5. Build deployment/installer
