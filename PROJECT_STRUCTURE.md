# LabGuard Solution Structure

```
LabGuard/
├── LabGuard.sln
├── README.md
├── LabGuard.Common/
│   ├── LabGuard.Common.csproj
│   ├── Protocol.cs               (Constants, enums, Client/Host info)
│   └── Messages.cs               (TCP message types & serialization)
├── LabGuard.Host/
│   ├── LabGuard.Host.csproj
│   ├── App.xaml                  (WPF entry point)
│   ├── App.xaml.cs
│   ├── MainWindow.xaml           (Bus topology UI)
│   ├── MainWindow.xaml.cs        (UI logic & sample data)
│   └── HostListener.cs           (TCP listener & message handler)
└── LabGuard.Client/
    ├── LabGuard.Client.csproj
    ├── Program.cs                (Entry point)
    └── NetworkClient.cs          (TCP client & heartbeat sender)
```

## Build Status

- `dotnet build` — Compiles all three projects
- `dotnet run --project LabGuard.Host` — Starts monitoring console
- `dotnet run --project LabGuard.Client` — Starts agent (sends heartbeats to localhost:9000)
