# LabGuard Implementation Summary

## Project Completion Status: ✅ MVP COMPLETE

This document describes the Implementation of the LabGuard Laboratory Monitoring System.

## Completed Features

### 1. **Network Communication Protocol** ✓
- **File**: `LabGuard.Common/Protocol.cs`, `Messages.cs`
- JSON serialization over TCP/IP
- Pre-Shared Key (PSK) authentication
- Message types: StatusUpdate, Command, Acknowledgement
- Length-prefixed message framing (4-byte big-endian length header)

### 2. **Client-Side Implementation** ✓
- **File**: `LabGuard.Client/NetworkClient.cs`
- Establishes persistent TCP connection to host
- Sends heartbeats every 10 seconds with:
  - System hostname
  - Running process count
  - Memory usage (MB)
- Listens concurrently for incoming commands
- Handles commands: Warn, Screenshot, Shutdown
- Sends acknowledgements after command execution
- Graceful error handling and reconnection logic

### 3. **Screenshot Capture** ✓
- **Implementation**: Screenshot execution in NetworkClient
- Captures primary screen using System.Drawing.Common
- Saves to: `%LOCALAPPDATA%\LabGuard\Evidence\screenshot_YYYYMMDD_HHMMSS.png`
- Automatic directory creation
- Error handling for access denied scenarios

### 4. **Process Monitoring** ✓
- **Implementation**: Heartbeat payload enhancement
- Monitors running processes using `System.Diagnostics.Process`
- Tracks system memory usage
- Reports in real-time status updates
- Gracefully handles permission issues

### 5. **Host Server** ✓
- **File**: `LabGuard.Host/HostListener.cs`
- Listens on port 9000 for incoming client connections
- Maintains dictionary of connected clients with sessions
- Tracks client status (Normal/Misuse/Offline)
- Monitors client health (30-second interval, 60-second timeout)
- Sends validated commands only (checks PSK)
- Supports multi-client management

### 6. **Host UI** ✓
- **File**: `LabGuard.Host/MainWindow.xaml(.cs)`
- WPF application for staff monitoring
- Displays live list of connected clients
- Real-time status updates (process count, memory usage)
- Interactive command buttons:
  - **Warn**: Send warning message to selected client
  - **Screenshot**: Trigger screenshot capture
  - **Shutdown**: Initiate system shutdown (60-second countdown)
- Dynamic client list (adds/removes as clients connect/disconnect)
- Thread-safe UI updates via Dispatcher

### 7. **Configuration & Security**
- PSK-based authentication (configurable in Protocol.cs)
- Supports localhost (127.0.0.1) and LAN configurations
- Configurable port (default: 9000)
- Message payload size limits (1MB max)

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Staff Monitor                         │
│              (LabGuard.Host - WPF App)                   │
│                                                          │
│  ┌──────────────────────────────────────────────────┐   │
│  │ MainWindow (UI)                                   │   │
│  │ - Client List Display                             │   │
│  │ - Command Buttons (Warn, Screenshot, Shutdown)    │   │
│  └──────────────────────────────────────────────────┘   │
│                         ▲                                │
│                         │ (TCP/IP via Network)           │
│                         ▼                                │
│  ┌──────────────────────────────────────────────────┐   │
│  │ HostListener (Backend)                            │   │
│  │ - Accepts Client Connections                      │   │
│  │ - Maintains Client Registry                       │   │
│  │ - Routes Commands to Clients                      │   │
│  └──────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
                         │
          ┌──────────────┴──────────────┬──────────┐
          │                             │          │
          ▼                             ▼          ▼
    ┌──────────────┐            ┌──────────────┐ ... (Multiple Clients)
    │ Student Lab  │            │ Student Lab  │
    │ Computer 1   │            │ Computer 2   │
    │              │            │              │
    │ ┌──────────┐ │            │ ┌──────────┐ │
    │ │ LabGuard │ │            │ │ LabGuard │ │
    │ │ Client   │ │            │ │ Client   │ │
    │ │ Agent    │ │            │ │ Agent    │ │
    │ │          │ │            │ │          │ │
    │ │ - Monitor│ │            │ │ - Monitor│ │
    │ │   Procs  │ │            │ │   Procs  │ │
    │ │ - Take   │ │            │ │ - Take   │ │
    │ │  Capture │ │            │ │  Capture │ │
    │ │ - Exec   │ │            │ │ - Exec   │ │
    │ │   Cmds   │ │            │ │   Cmds   │ │
    │ └──────────┘ │            │ └──────────┘ │
    └──────────────┘            └──────────────┘
```

## Message Flow Example

```
Client                          Host
  │                             │
  │──[StatusUpdate]────────────→│
  │  (hostname, proc_count)     │  → Updates Client List
  │                             │
  │←────────[Command]───────────│
  │  (take screenshot)          │
  │                             │
  │  [Execute: Screenshot]      │
  │  [Save to Evidence dir]     │
  │                             │
  │──[Acknowledgement]─────────→│
  │  (command completed)        │
  │                             │
```

## Technology Stack

| Component      | Technology                           |
|----------------|--------------------------------------|
| Framework      | .NET 8.0 (net8.0-windows)            |
| GUI            | WPF (Windows Presentation Foundation)|
| Networking     | TCP/IP Sockets                       |
| Serialization  | System.Text.Json                     |
| Capture        | System.Drawing.Common                |
| Process Info   | System.Diagnostics.Process           |
| UI Threading   | System.Threading + Dispatcher        |

## File Structure

```
LabGuard/
├── LabGuard.sln
├── README.md
├── PROJECT_STRUCTURE.md
├── IMPLEMENTATION.md                (This file)
├── SETUP.md
│
├── LabGuard.Common/
│   ├── LabGuard.Common.csproj
│   ├── Protocol.cs                  (PSK, enums, ClientInfo)
│   └── Messages.cs                  (Serialization, message types)
│
├── LabGuard.Host/
│   ├── LabGuard.Host.csproj
│   ├── App.xaml / App.xaml.cs       (WPF app entry)
│   ├── MainWindow.xaml              (UI layout)
│   ├── MainWindow.xaml.cs           (UI logic & event handlers)
│   └── HostListener.cs              (TCP listener & client manager)
│
└── LabGuard.Client/
    ├── LabGuard.Client.csproj       (NuGet refs: Sys.Drawing, Sys.WinForms)
    ├── Program.cs                   (Entry point)
    └── NetworkClient.cs             (TCP client, heartbeat, command handling)
```

## Configuration & Customization

### Change Pre-Shared Key (PSK)
Edit `LabGuard.Common/Protocol.cs`:
```csharp
public const string PSK = "YOUR_SECURE_PSK_HERE";
```

### Change Server Port
Edit `LabGuard.Host/HostListener.cs`:
```csharp
public HostListener(int port = 9000)  // Change 9000 to desired port
```

### Change Server Address (Client-Side)
Edit `LabGuard.Client/Program.cs`:
```csharp
var client = new NetworkClient("192.168.1.100", 9000);  // Change IP and port
```

### Screenshot Storage Location
Current: `%LOCALAPPDATA%\LabGuard\Evidence\`
Editable in `LabGuard.Client/NetworkClient.cs` in `ExecuteScreenshot()`.

## Testing Instructions

### Build Everything
```bash
cd c:\23g133\Project\Labguard
dotnet build
```

### Test Scenario 1: Local Loopback (Same Machine)
**Terminal 1:**
```bash
dotnet run --project LabGuard.Host
```

**Terminal 2:**
```bash
dotnet run --project LabGuard.Client
```

**Expected:**
- Host shows client "localhost" (or machine name) in the list
- Every 10 seconds: heartbeat with process count and memory
- Host UI is interactive: select client and click buttons

### Test Scenario 2: Multi-Machine LAN
1. Edit `LabGuard.Client/Program.cs` to connect to Host machine IP
2. Run Host on management machine
3. Run Client on lab machines
4. Monitor all connected clients in Host UI

## Known Limitations & Future Enhancements

### Current Limitations
- No TLS/SSL encryption (PSK only)
- Single-threaded command processing per client
- No persistent logging database
- No screenshot thumbnail preview in UI
- Shutdown command uses native Windows API (may vary by locale)

### Planned Enhancements
- [ ] Browser activity tracking (HTTP/HTTPS log hooks)
- [ ] Windows Service installation for client (auto-start)
- [ ] Encrypted communication (TLS)
- [ ] Screenshot evidence gallery viewer in UI
- [ ] Process activity timeline/history
- [ ] Remote code execution (with safety restrictions)
- [ ] Role-based access control (multi-admin support)
- [ ] Database persistence (SQL Server Express)
- [ ] Email/SMS alerts for suspicious activity
- [ ] Integration with Active Directory

## Performance Considerations

- **Heartbeat Interval**: 10 seconds (configurable in NetworkClient.cs)
- **Health Monitor Interval**: 30 seconds
- **Offline Timeout**: 60 seconds (no heartbeat = client removed)
- **Message Size Limit**: 1MB (security measure)
- **Max Connected Clients**: Limited by OS socket limits (typically 5000+)

## Security Considerations

### Before Production:
1. **Change PSK** - Use strong, random 32+ character string
2. **Enable TLS** - Wrap TCP connection with SSL/TLS
3. **Firewall Rules** - Restrict port 9000 to authenticated networks only
4. **File Permissions** - Restrict Evidence directory access
5. **Code Signing** - Sign executables to prevent tampering
6. **Rate Limiting** - Add per-client command rate limits
7. **Logging** - Implement audit trail for all commands

### Current Threat Model
- **Assumes**: Trusted LAN network
- **Vulnerable to**: Network sniffing (no encryption), PSK brute force, man-in-the-middle
- **Mitigated by**: PSK authentication (better than nothing)

## Troubleshooting

### Client won't connect
- [ ] Check firewall on Host machine (port 9000)
- [ ] Verify Host IP address in NetworkClient constructor
- [ ] Check PSK matches in both projects
- [ ] Ensure both running .NET 8.0+

### Commands don't execute
- [ ] Check host console for "Command [X] sent to [Client]"
- [ ] Ensure client is selected in UI before clicking button
- [ ] Check PSK validation in HostListener.cs

### Screenshots not saving
- [ ] Check `%LOCALAPPDATA%\LabGuard\Evidence\` directory exists
- [ ] Verify file permissions in Evidence directory
- [ ] Check disk space availability

## Project Statistics

| Metric                    | Count |
|---------------------------|-------|
| C# Source Files           | 8     |
| Lines of Code (Core)      | ~800  |
| Classes/Types             | 10    |
| Message Types             | 3     |
| Real-time Connections     | ∞     |
| Features Implemented      | 7/12  |

## Authors & License

- **Project**: LabGuard v1.0 (MVP)
- **Created**: February 2026
- **Framework**: .NET 8.0
- **License**: Internal Use Only

---

**Last Updated**: February 10, 2026
**Status**: ✅ MVP Ready for Testing

