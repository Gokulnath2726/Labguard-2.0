# LabGuard - Batch File Setup Guide

## Quick Start with Batch Files

This folder contains 3 batch files for easy LabGuard deployment:

---

## 1. **START_HOST.bat** (For Staff/Host Computer)
**Location:** Host computer (monitoring console)

### How to use:
1. Double-click `START_HOST.bat`
2. Host application starts with animated logo
3. **Keep this window OPEN** while using the system
4. All lab computers will connect here

**What you'll see:**
```
LabGuard Host Console
Listening on port 9000...
All lab computers will connect here.
```

---

## 2. **START_CLIENT.bat** (For Lab Computers - With Prompt)
**Location:** Each student/lab computer

### How to use:
1. Double-click `START_CLIENT.bat`
2. **Enter the Host IP address** when prompted
   - Example: `192.168.1.10`
   - Press ENTER
3. Client connects to Host
4. Console shows heartbeat messages

**What you'll see:**
```
Enter the Host/Server IP Address:
Host IP Address (example: 192.168.1.10): 192.168.1.10

Connecting to Host at: 192.168.1.10:9000
```

---

## 3. **START_CLIENT_AUTO.bat** (For Lab Computers - Auto)
**Location:** Each student/lab computer (if Host IP is always the same)

### How to use:
1. **Edit the file first:**
   - Right-click → Edit
   - Find line: `set HOST_IP=192.168.1.10`
   - Change `192.168.1.10` to your HOST IP
   - Save

2. Double-click `START_CLIENT_AUTO.bat`
3. **It automatically connects** - no input needed!

**Best for:** Labs with fixed, unchanging Host IP

---

## Lab Setup Example

### **Lab Network Configuration:**

| Computer | Role | IP | Batch File |
|----------|------|-----|-----------|
| Staff Computer | **HOST** | 192.168.1.10 | `START_HOST.bat` |
| Lab PC 01 | Client | 192.168.1.101 | `START_CLIENT.bat` or `START_CLIENT_AUTO.bat` |
| Lab PC 02 | Client | 192.168.1.102 | `START_CLIENT.bat` or `START_CLIENT_AUTO.bat` |
| Lab PC 03 | Client | 192.168.1.103 | `START_CLIENT.bat` or `START_CLIENT_AUTO.bat` |

---

## Step-by-Step Lab Deployment

### **Step 1: On Host/Staff Computer**
1. Copy `START_HOST.bat` to Host computer
2. Double-click to start Host
3. Window shows: "Listening on port 9000..."
4. **Keep it running**

### **Step 2: On Each Lab Computer**
1. Copy `START_CLIENT.bat` or `START_CLIENT_AUTO.bat` to each lab PC
2. Double-click batch file
3. Enter Host IP (or auto-connects if using AUTO version)
4. Console shows: "Connected to host"
5. Every 10 seconds you see: "Heartbeat sent..."

### **Step 3: In Host Window**
- Each client appears in the list
- Shows process count and memory
- Buttons become clickable: Warn, Screenshot, Shutdown

---

## Requirements

✅ Windows 7 or later  
✅ .NET 8 Runtime (install from https://dotnet.microsoft.com/download)  
✅ Network connectivity between computers  

---

## Troubleshooting

### **Client won't connect:**
1. Check Host is running first
2. Verify Host IP is correct
3. Check firewall allows port 9000
4. Ensure both on same network

### **Commands don't work:**
1. Make sure client is selected in Host window
2. Check both are on same network
3. Verify PSK matches (if modified)

### **"Connection refused" error:**
- Host is not running
- Wrong IP address
- Firewall blocking port 9000

---

## Customization

### For Multiple Labs:
Create separate batch files for each lab:
- `START_CLIENT_LAB1.bat` (connects to Lab1 Host IP)
- `START_CLIENT_LAB2.bat` (connects to Lab2 Host IP)
- `START_CLIENT_LAB3.bat` (connects to Lab3 Host IP)

### Security:
Before production, edit `Program.cs` in source to change:
```
PSK = "YOUR_SECURE_PASSWORD"
```

---

## File Locations

If batch files are on USB or different folder, update paths:

**In START_HOST.bat**, change:
```batch
C:\23g133\Project\Labguard\LabGuard.Host\bin\Release\net8.0-windows\LabGuard.Host.exe
```
To your path.

**In START_CLIENT.bat**, change:
```batch
C:\23g133\Project\Labguard\LabGuard.Client\bin\Release\net8.0-windows\LabGuard.Client.exe
```
To your path.

---

## Support

For issues, check:
1. Console error messages
2. Network connectivity
3. .NET 8 installation
4. Firewall settings

---

**Ready to deploy!** 🚀

